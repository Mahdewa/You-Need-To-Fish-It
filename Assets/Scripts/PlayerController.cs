using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    // --- Variabel Gerakan ---
    public float moveSpeed = 5f;
    private Rigidbody2D rbPlayer;
    private SpriteRenderer playerSprite;
    private Collider2D playerCollider; 

    // --- Variabel Perahu ---
    public Rigidbody2D rbBoat; 
    public Transform boatSeatPosition;
    
    [Header("Posisi Parkir")]
    public Transform playerLandPosition;
    public Transform boatStartPosition;

    // --- Status Player ---
    private bool isOnBoat = false;
    private bool isAtDock = false; 
    private bool isInFishingZone = false;
    private bool isInBaitZone = false;
    private bool isBusy = false; // Mencegah spam aksi (mancing, gali)
    private bool canRestAtTent = false;
    private bool canTalkToNPC = false;
    private bool isAtBigBoatDock = false;
    private bool isInteractingWithNPC = false;
    private InteractionPromptAsset currentPromptAssetScript;

    public bool isUIOpen = false;

    // --- Variabel Sistem Memancing ---
    [Header("Sistem Memancing")]
    public float fishingWaitTime = 3.0f; 
    public float fishingEnergyCostDock = 10f; // Biaya di Dermaga (Murah)
    public float fishingEnergyCostSea = 25f;  // Biaya di Laut (Mahal)
    public float bridgeFishingSuccessChance = 0.5f; 
    public float seaFishingSuccessChance = 0.8f;

    [Header("Sistem Mash Button (UI)")]
    public GameObject mashUIContainer;   // Masukkan GameObject 'BarBackground' atau Canvas-nya di sini
    public Image mashProgressBar;        // Masukkan Image 'BarFill' di sini
    public float mashIncreaseAmount = 0.15f; // Berapa banyak bar nambah saat diklik (0.15 = 15%)
    public float mashDecaySpeed = 0.3f;

    [Header("Kesulitan Ikan (Decay Speed)")]
    public float smallFishDecay = 0.1f;  // Mudah
    public float mediumFishDecay = 0.25f; // Sedang
    public float bigFishDecay = 0.45f;    // Susah (Cepat turunnya)

    // --- Variabel Sistem Bait ---
    [Header("Sistem Bait")]
    public float baitSuccessChance = 0.5f; // 30%
    public float baitEnergyCost = 5f;
    public float baitHoldTime = 2.0f; // Waktu (detik) untuk menahan tombol

    // --- ISTIRAHAT ---
    [Header("Sistem Istirahat")]
    public float restTime = 1.5f;

    // --- TOKO NPC ---
    [Header("Toko NPC")]
    public int rawFishPrice = 10;
    public int smallBoatPrice = 100; // Sesuai permintaan Anda
    public int bigBoatPrice = 1000;  // Objektif utama

    [Header("Sistem Toko")]
    public int baitPrice = 10;
    public int energyFoodPrice = 20;
    public float energyFoodRestoreAmount = 30f; // Berapa banyak energi yang dipulihkan
    private bool isInShopZone = false;
    
    // --- RUNNING SFX ---
    [Header("Running SFX")]
    [SerializeField] private float runningSfxInterval = 0.38f; // Interval untuk play running SFX
    private float runningSfxTimer = 0f;
    private bool isRunningSfxPlaying = false; // Track apakah running SFX sedang aktif
    
    // --- ANIMASI ---
    private PlayerAnimator playerAnimator;
    
    private enum FishingState { None, Casting, Waiting, Hooked, Reeling, Result }
    private FishingState currentFishingState = FishingState.None;


    void Start()
    {
        rbPlayer = GetComponent<Rigidbody2D>(); 
        playerSprite = GetComponent<SpriteRenderer>();
        playerCollider = GetComponent<Collider2D>();
        playerAnimator = GetComponent<PlayerAnimator>();
    }

    void Update()
    {
        if (currentFishingState != FishingState.None) return;
        if (isBusy) return;
        if (isUIOpen) return;

        if (isInShopZone)
        {
            // Tombol Q untuk Beli Umpan
            if (Input.GetKeyDown(KeyCode.Q))
            {
                BuyBaitFromShop();
            }
            // Tombol E untuk Beli Makanan (Energi)
            if (Input.GetKeyDown(KeyCode.E))
            {
                BuyFoodFromShop();
            }
        }

        // --- Sistem Kapal ---
        if (isAtBigBoatDock && Input.GetKeyDown(KeyCode.E)) {
            if (InventorySystem.instance.ownsBigBoat) {
                // Jika sudah, tamatkan game
                WinGame();
            }
            else {
                UIManager.instance.ShowPlayerBubble("I NEED TO BUY THIS SHIP!");
            }
            return;
        }
        // --- Naik Perahu ---
        if (isAtDock && Input.GetKeyDown(KeyCode.E)) {
            if (InventorySystem.instance.hasSmallBoat)
            {
                ToggleBoatMode();
            }
            else
            {
                UIManager.instance.ShowPlayerBubble("I NEED TO BUY THIS BOAT!");
                // (Nanti bisa ganti UIManager.instance.ShowNotification dengan memunculkan UI Text)
            }
            return;
        }
        // --- SISTEM MENCARI UMPAN (DIUBAH) ---
        if (isInBaitZone && !isOnBoat && Input.GetKeyDown(KeyCode.E))
        {
            StartCoroutine(StartBaitSequence());
            return;
        }
        if (canTalkToNPC && !isOnBoat && Input.GetKeyDown(KeyCode.E))
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX("button");
            }
            else
            {
                Debug.LogError("AudioManager tidak ditemukan! Pastikan scene Main Menu dijalankan pertama kali.");
            }
            // Set status "sedang berinteraksi"
            isInteractingWithNPC = true;
            
            // Perbarui UI Konteks untuk menampilkan opsi Jual/Beli
            if (!InventorySystem.instance.hasSmallBoat) {
                // Tampilkan aset untuk "Beli Perahu Kecil"
                if (currentPromptAssetScript != null && currentPromptAssetScript.boatPurchasePrompt != null) {
                    UIManager.instance.ShowPersistentNotification(currentPromptAssetScript.boatPurchasePrompt);
                }
            } else {
                // Tampilkan aset untuk "Beli Kapal Besar"
                if (currentPromptAssetScript != null && currentPromptAssetScript.shipPurchasePrompt != null) {
                    UIManager.instance.ShowPersistentNotification(currentPromptAssetScript.shipPurchasePrompt);
                }
            }
            return; // Hentikan frame
        }
        // --- SISTEM ISTIRAHAT ---
        if (canRestAtTent && !isOnBoat && Input.GetKeyDown(KeyCode.R))
        {
            AudioManager.Instance.PlaySFX("button");
            StartCoroutine(RestSequence());
            return;
        }
        // --- SISTEM MEMANCING ---
        if (isInFishingZone && Input.GetMouseButtonDown(0))
        {
            StartCoroutine(StartFishingSequence());
            return;
        }
        if (isInteractingWithNPC && !isOnBoat)
        {
            // Tombol 'J' untuk JUAL
            if (Input.GetKeyDown(KeyCode.J)) {
                AudioManager.Instance.PlaySFX("button");
                SellAllRawFish();
            }
            // Tombol 'B' untuk BELI
            if (Input.GetKeyDown(KeyCode.B)) {
                AudioManager.Instance.PlaySFX("button");
                if (!InventorySystem.instance.hasSmallBoat) { BuySmallBoat(); }
                else { BuyBigBoat(); }
            }
        }
    }

    void FixedUpdate()
    {
        if (isUIOpen) return;
        // JANGAN bergerak jika sedang sibuk (mancing/gali)
        if (isBusy || currentFishingState != FishingState.None)
        {
            rbPlayer.linearVelocity = new Vector2(0, rbPlayer.linearVelocity.y); // Hentikan gerak X
            rbBoat.linearVelocity = Vector2.zero;
            runningSfxTimer = 0f; // Reset timer
            
            // Stop running SFX jika sedang aktif
            if (isRunningSfxPlaying)
            {
                AudioManager.Instance.StopSFX();
                isRunningSfxPlaying = false;
            }
            
            return;
        }
        
        float moveInput = Input.GetAxis("Horizontal");
        
        if (isOnBoat)
        {
            rbBoat.linearVelocity = new Vector2(moveInput * moveSpeed, 0);
            playerAnimator.PlayWalk(moveInput);
            playerAnimator.FlipSprite(moveInput);
            runningSfxTimer = 0f; // Reset timer saat di perahu
            
            // Stop running SFX jika sedang aktif
            if (isRunningSfxPlaying)
            {
                AudioManager.Instance.StopSFX();
                isRunningSfxPlaying = false;
            }
        }
        else
        {
            rbPlayer.linearVelocity = new Vector2(moveInput * moveSpeed, rbPlayer.linearVelocity.y);
            
            // --- INTEGRASI ANIMASI ---
            if (moveInput != 0)
            {
                playerAnimator.PlayWalk(moveInput);
            }
            else
            {
                playerAnimator.PlayIdle();
            }
            playerAnimator.FlipSprite(moveInput);
            
            // --- RUNNING SFX LOGIC ---
            if (moveInput != 0) // Player sedang bergerak
            {
                // Jika running SFX belum aktif, start sekarang
                if (!isRunningSfxPlaying)
                {
                    isRunningSfxPlaying = true;
                    runningSfxTimer = 0f;
                }
                
                runningSfxTimer += Time.deltaTime;
                
                // Mainkan SFX setiap interval
                if (runningSfxTimer >= runningSfxInterval)
                {
                    AudioManager.Instance.PlaySFX("lari");
                    runningSfxTimer = 0f;
                }
            }
            else // Player berhenti
            {
                // Stop running SFX jika sedang aktif
                if (isRunningSfxPlaying)
                {
                    AudioManager.Instance.StopSFX();
                    isRunningSfxPlaying = false;
                }
                
                runningSfxTimer = 0f; // Reset timer
            }
        }
    }

private FishSize DetermineFishSize() {
        float roll = Random.value; // Angka acak 0.0 sampai 1.0

        if (isOnBoat) {
            // --- LAUTAN (Boat) ---
            // 40% Besar, 40% Sedang, 20% Kecil
            if (roll <= 0.3f) return FishSize.Big;       // 0 - 0.4 (40%)
            else if (roll <= 0.75f) return FishSize.Medium; // 0.4 - 0.8 (40%)
            else return FishSize.Small;                    // 0.8 - 1.0 (20%)
        } 
        else {
            // --- PELABUHAN/JEMBATAN (Land) ---
            // 10% Besar, 30% Sedang, 60% Kecil
            if (roll <= 0.1f) return FishSize.Big;         // 0 - 0.1 (10%)
            else if (roll <= 0.5f) return FishSize.Medium; // 0.1 - 0.4 (30%)
            else return FishSize.Small;                    // 0.4 - 1.0 (60%)
        }
    }


    // --- Fungsi BARU: Jual Ikan ---
private void SellAllRawFish() {
        // 1. Hitung dulu berapa jumlah ikan yang akan dijual
        int totalFishToSell = InventorySystem.instance.GetTotalFishCount();

        // 2. Jual (Dapatkan Uang)
        int moneyEarned = InventorySystem.instance.SellAllFish();
        
        if (moneyEarned > 0) {
            UIManager.instance.ShowPlayerBubble($"SOLD FOR ${moneyEarned}!");
            AudioManager.Instance.PlaySFX("button");
            
            // 3. LAPOR KE DAILY MISSION (PENTING!)
            if (DailyMissionManager.instance != null) {
                DailyMissionManager.instance.AddProgress(totalFishToSell);
            }
        } else {
            UIManager.instance.ShowPlayerBubble("NO FISH TO SELL");
        }
    }

    private void BuySmallBoat() {
        bool success = InventorySystem.instance.PurchaseItem(smallBoatPrice);
        if (success) {
            InventorySystem.instance.hasSmallBoat = true;
            UIManager.instance.ShowPlayerBubble("THE BOAT IS MINE NOW >.<");
        }
        else {
            int coinsNeeded = smallBoatPrice - InventorySystem.instance.money;
            UIManager.instance.ShowPlayerBubble($"{coinsNeeded} MORE FOR A BOAT");
        }
    }

    // --- Fungsi BARU: Beli Kapal Besar (Objektif) ---
    private void BuyBigBoat() {
        // Cek dulu apakah sudah punya
        if (InventorySystem.instance.ownsBigBoat) {
            UIManager.instance.ShowPlayerBubble("I ALREADY OWN IT!");
            return;
        }
        
        // Panggil fungsi 'PurchaseItem' yang baru
        bool success = InventorySystem.instance.PurchaseItem(bigBoatPrice);
        if (success) {
            InventorySystem.instance.ownsBigBoat = true;
            UIManager.instance.ShowPlayerBubble("NOW I CAN GO TO THE MAIN ISLAND!");
        }
        else {
            int coinsNeeded = bigBoatPrice - InventorySystem.instance.money;
            UIManager.instance.ShowPlayerBubble($"{coinsNeeded} MORE FOR A MOTORBOAT");
        }
    }

    // --- FUNGSI BARU: BELI DI TOKO ---
    private void BuyBaitFromShop()
    {
        if (InventorySystem.instance.money >= baitPrice)
        {
            InventorySystem.instance.SpendMoney(baitPrice);
            InventorySystem.instance.AddBait(1);
            AudioManager.Instance.PlaySFX("button"); // Atau SFX 'buy'
            UIManager.instance.ShowPlayerBubble("BAIT +1");
        }
        else
        {
            UIManager.instance.ShowPlayerBubble("NO MONEY!");
        }
    }

    private void BuyFoodFromShop()
    {
        // Cek apakah energi sudah penuh? (Opsional, kalau mau boros boleh aja beli terus hehe)
        if (EnergySystem.instance.currentEnergy >= EnergySystem.instance.maxEnergy)
        {
            UIManager.instance.ShowPlayerBubble("I'M FULL!");
            return;
        }

        if (InventorySystem.instance.money >= energyFoodPrice)
        {
            InventorySystem.instance.SpendMoney(energyFoodPrice);
            
            // Panggil fungsi AddEnergy (Pastikan EnergySystem punya ini)
            EnergySystem.instance.AddEnergy(energyFoodRestoreAmount);
            
            AudioManager.Instance.PlaySFX("button"); // Atau SFX 'eat'
            UIManager.instance.ShowPlayerBubble("DELICIOUS!");
        }
        else
        {
            UIManager.instance.ShowPlayerBubble("NO MONEY!");
        }
    }

    // --- Coroutine BARU: Istirahat ---
// --- Coroutine BARU: Istirahat ---
    private IEnumerator RestSequence()
    {
        // 1. CEK DULU: APAKAH MISI SUDAH SELESAI?
        if (DailyMissionManager.instance != null)
        {
            if (!DailyMissionManager.instance.CanSleep())
            {
                UIManager.instance.ShowPlayerBubble("I HAVEN'T FINISHED MY JOB..."); 
                yield return new WaitForSeconds(1f); // Jeda sebentar
                yield break; // Game Over akan dipanggil oleh Manager
            }
        }

        // --- Kalau Misi Selesai, Lanjut Tidur ---
        isBusy = true; 
        playerAnimator.PlayIdle(); 
        
        UIManager.instance.ShowRestingImage();
        yield return new WaitForSeconds(restTime);

        EnergySystem.instance.RestoreAllEnergy();
        UIManager.instance.HideRestingImage();
        
        // Pindah Hari
        TimeSystem.instance.PassDay();

        // 2. SIAPKAN MISI UNTUK BESOK (RESET MISI)
        if (DailyMissionManager.instance != null) {
            int newDay = TimeSystem.instance.GetCurrentDay();
            DailyMissionManager.instance.StartNewDayMission(newDay);
        }
        
        // >>> 3. AUTO-SAVE (INI PERBAIKANNYA!) <<<
        // Simpan data tepat setelah bangun tidur (Hari Baru + Misi Baru)
        if (SaveSystem.instance != null) {
            SaveSystem.instance.SaveGameData();
            Debug.Log("Auto-Save: Data hari baru tersimpan!");
        }

        isBusy = false; 
    }

    // --- Coroutine MENCARI UMPAN (PERUBAHAN BESAR DI SINI) ---
    private IEnumerator StartBaitSequence() {
        isBusy = true; // Player sibuk, tidak bisa bergerak
        UIManager.instance.ShowPlayerBubble("!!??");
        playerAnimator.PlayBaiting(); // Trigger baiting animation

        float timer = 0f;
        float sfxTimer = 0f; // Timer untuk SFX loop
        float sfxInterval = 0.8f; // Interval untuk memainkan SFX (setiap 0.3 detik)

        // (Di sini Anda bisa memunculkan UI Progress Bar)
        // progressBar.fillAmount = 0;

        // --- Loop Selama Tombol Ditekan ---
        while (timer < baitHoldTime) {
            // Cek 1: Apakah tombol 'E' MASIH ditekan?
            if (!Input.GetKey(KeyCode.E)) {
                UIManager.instance.ShowPlayerBubble("NEVERMIND");
                isBusy = false;
                playerAnimator.PlayIdle(); // Return to idle
                // (Sembunyikan UI Progress Bar)
                yield break; // Hentikan coroutine
            }

            // Cek 2: Apakah player masih di zona bait?
            if (!isInBaitZone) {
                isBusy = false;
                playerAnimator.PlayIdle(); // Return to idle
                // (Sembunyikan UI Progress Bar)
                yield break; // Hentikan coroutine
            }

            // Lanjutkan timer
            timer += Time.deltaTime;
            sfxTimer += Time.deltaTime;

            // Mainkan SFX setiap interval
            if (sfxTimer >= sfxInterval) {
                AudioManager.Instance.PlaySFX("baiting");
                sfxTimer = 0f; // Reset sfx timer
            }
            
            // (Update UI Progress Bar di sini)
            // progressBar.fillAmount = timer / baitHoldTime;

            yield return null; // Tunggu frame berikutnya
        }

        // --- Jika loop selesai, artinya player BERHASIL menahan tombol ---
        // UIManager.instance.ShowNotification("Selesai menahan tombol. Memeriksa hasil...");

        // Cek Energi
        if (!EnergySystem.instance.HasEnoughEnergy(baitEnergyCost)) {
            UIManager.instance.ShowPlayerBubble("I'M TIRED.");
            isBusy = false;
            playerAnimator.PlayIdle(); // Return to idle
            yield break; // Hentikan
        }

        // Gunakan Energi
        EnergySystem.instance.UseEnergy(baitEnergyCost);

        // HAPUS: 'yield return new WaitForSeconds(baitSearchTime);'
        // Timer 'while' di atas sudah menggantikan 'WaitForSeconds'

        // Tentukan Hasil (30% chance)
        if (Random.value <= baitSuccessChance) {
            // BERHASIL
            InventorySystem.instance.AddBait(1);
            UIManager.instance.ShowPlayerBubble("YEAYY!!!");

        }
        else {
            // GAGAL
            UIManager.instance.ShowPlayerBubble("HUFT, UNLUCKY.");
        }

        isBusy = false; // Selesai, player bisa gerak lagi
        playerAnimator.PlayIdle(); // Return to idle
        // (Sembunyikan UI Progress Bar)
    }

    // --- Coroutine Memancing ---
// --- COROUTINE MEMANCING (DENGAN TIPE IKAN) ---
   private IEnumerator StartFishingSequence() {
        // 1. TENTUKAN BIAYA ENERGI
        float currentEnergyCost = isOnBoat ? fishingEnergyCostSea : fishingEnergyCostDock;

        // 2. CEK KETERSEDIAAN SUMBER DAYA
        if (!InventorySystem.instance.HasBait(1)) {
            UIManager.instance.ShowPlayerBubble("NO BAIT");
            yield break;
        }

        if (!EnergySystem.instance.HasEnoughEnergy(currentEnergyCost)) {
            UIManager.instance.ShowPlayerBubble("I'M TIRED"); 
            yield break;
        }
        
        // 3. GUNAKAN SUMBER DAYA
        InventorySystem.instance.UseBait(1);
        EnergySystem.instance.UseEnergy(currentEnergyCost); 
        
        isBusy = true; 
        currentFishingState = FishingState.Casting;
        playerAnimator.PlayFishingStart(); 
        AudioManager.Instance.PlaySFX("mancing-awal");
        UIManager.instance.ShowPlayerBubble("THROWING..."); 
        yield return new WaitForSeconds(0.5f); 

        currentFishingState = FishingState.Waiting;
        playerAnimator.PlayFishingWaiting(); 
        UIManager.instance.ShowPlayerBubble("...");
        yield return new WaitForSeconds(fishingWaitTime); 

        // --- PHASE 1: HOOKED ---
        currentFishingState = FishingState.Hooked;
        AudioManager.Instance.PlaySFX("button");
        CameraShake.Shake(duration: 0.3f, magnitude: 0.1f);
        
        FishSize hookedFish = DetermineFishSize();
        
        if (hookedFish == FishSize.Big) UIManager.instance.ShowPlayerBubble("!!! HEAVY !!!");
        else UIManager.instance.ShowPlayerBubble("!");

        float reactionTimer = 1.0f; 
        bool hookSuccess = false;
        while (reactionTimer > 0) {
            if (Input.GetMouseButtonDown(0)) { hookSuccess = true; break; }
            reactionTimer -= Time.deltaTime;
            yield return null;
        }

        if (!hookSuccess) {
            UIManager.instance.ShowPlayerBubble("TOO SLOW...");
            playerAnimator.PlayFishingEnd(); 
            isBusy = false; 
            currentFishingState = FishingState.None; 
            yield break; 
        }

        // --- PHASE 2: REELING (MASH BUTTON) ---
        currentFishingState = FishingState.Reeling;
        playerAnimator.PlayFishingEnd(); 
        UIManager.instance.ShowPlayerBubble("MASH CLICK!!"); 
        
        if (mashUIContainer != null) mashUIContainer.SetActive(true);
        
        // Atur Kesulitan
        float currentDecay = 0f;
        switch (hookedFish) {
            case FishSize.Small: currentDecay = smallFishDecay; break;
            case FishSize.Medium: currentDecay = mediumFishDecay; break;
            case FishSize.Big: currentDecay = bigFishDecay; break;
        }

        float currentProgress = 0.3f; 
        bool fishCaught = false;
        bool fishLost = false;

        // --- VARIABEL UNTUK LOOPING SFX (BARU) ---
        float reelingSfxTimer = 0f;
        // Atur interval ini sesuai durasi audio clip 'mancing-reeling' kamu 
        // (Misal clip-nya 0.8 detik, set ini jadi 0.8f agar loopingnya mulus)
        float reelingSfxInterval = 0.8f; 
        
        // Putar SFX pertama kali saat masuk fase reeling
        AudioManager.Instance.PlaySFX("mancing-reeling");

        while (!fishCaught && !fishLost) {
            currentProgress -= currentDecay * Time.deltaTime;

            // --- LOGIKA LOOP SFX ---
            reelingSfxTimer += Time.deltaTime;
            if (reelingSfxTimer >= reelingSfxInterval) {
                AudioManager.Instance.PlaySFX("mancing-reeling");
                reelingSfxTimer = 0f; // Reset timer
            }

            if (Input.GetMouseButtonDown(0)) {
                currentProgress += mashIncreaseAmount;
                CameraShake.Shake(duration: 0.1f, magnitude: 0.05f); 
            }

            if (mashProgressBar != null) mashProgressBar.fillAmount = currentProgress;

            if (currentProgress >= 1.0f) fishCaught = true;
            else if (currentProgress <= 0.0f) fishLost = true;

            yield return null; 
        }
        
        // --- HENTIKAN SFX SEGERA SETELAH SELESAI (BARU) ---
        AudioManager.Instance.StopSFX();

        if (mashUIContainer != null) mashUIContainer.SetActive(false);

        // --- PHASE 3: HASIL ---
        if (fishCaught) {
            AudioManager.Instance.PlaySFX("mancing-berhasil");
            
            InventorySystem.instance.AddFish(hookedFish, 1);
            
            if (CollectionManager.instance != null) {
                FishData caughtFish = CollectionManager.instance.GetRandomFish(hookedFish);
                bool isNewDiscovery = CollectionManager.instance.UnlockFish(caughtFish.fishName);

                if (isNewDiscovery) {
                    UIManager.instance.ShowPlayerBubble($"YEAYY, NEW FISH XD");
                } 
                else {
                    UIManager.instance.ShowPlayerBubble($"I GOT {caughtFish.fishName}!");
                }
            }
            else {
                UIManager.instance.ShowPlayerBubble("GOT A FISH!");
            }
            
            CameraShake.Shake(duration: 0.3f, magnitude: 0.4f);
        }
        else {
            AudioManager.Instance.PlaySFX("mancing-gagal");
            UIManager.instance.ShowPlayerBubble("IT ESCAPED..."); 
        }

        yield return new WaitForSeconds(1.0f); 
        playerAnimator.PlayIdle(); 
        isBusy = false; 
        currentFishingState = FishingState.None;
    }
    
        // --- Logika Transisi Perahu ---
    private void ToggleBoatMode()
    {
        isOnBoat = !isOnBoat; 
        if (isOnBoat)
        {
            rbPlayer.isKinematic = true; 
            playerCollider.isTrigger = true; 
            this.transform.SetParent(rbBoat.transform); 
            this.transform.localPosition = boatSeatPosition.localPosition; 
            rbPlayer.linearVelocity = Vector2.zero;
            playerAnimator.SetBoatState(true); // Set boat idle animation
        }
        else
        {
            rbPlayer.isKinematic = false; 
            playerCollider.isTrigger = false; 
            this.transform.SetParent(null); 
            this.transform.position = playerLandPosition.position; 
            rbBoat.transform.position = boatStartPosition.position;
            rbBoat.linearVelocity = Vector2.zero;
            playerAnimator.SetBoatState(false); // Set land idle animation
        }
    }

    // --- Logika Trigger (Tidak Berubah) ---
    private void OnTriggerEnter2D(Collider2D other) {
        // 1. Coba dapatkan script aset dari trigger
        // (Kita gunakan GetComponent, bukan GetCom...InChildren, karena script-nya ada di trigger itu sendiri)
        InteractionPromptAsset assetScript = other.GetComponent<InteractionPromptAsset>();

        // 2. Jika trigger ini punya script aset, tampilkan aset 'default'-nya
        if (assetScript != null && assetScript.defaultPrompt != null) 
        {
            UIManager.instance.ShowPersistentNotification(assetScript.defaultPrompt);
            currentPromptAssetScript = assetScript; // <-- Simpan referensinya
        }

        // --- TAG BARU: ShopZone ---
        if (other.CompareTag("ShopZone")) {
            isInShopZone = true;
            // Tampilkan notifikasi cara beli
            // Kalau kamu pakai sistem PromptAsset, set di Inspector unity-nya.
            // UIManager.instance.ShowPersistentNotification(shopText);
        }

        if (other.CompareTag("Dock")) {
            isAtDock = true;
            // UIManager.instance.ShowPersistentNotification("TEKAN 'E' UNTUK NAIK PERAHU.");
        }
        if (other.CompareTag("BigBoatDock")) {
            isAtBigBoatDock = true;
            // UIManager.instance.ShowPersistentNotification("TEKAN 'E' UNTUK NAIK KAPAL.");
        }
        if (other.CompareTag("FishingZone")) { 
            // UIManager.instance.ShowPersistentNotification("KLIK KIRI UNTUK MULAI MEMANCING.");
            isInFishingZone = true; 
        }
        if (other.CompareTag("BaitZone")) {
            // UIManager.instance.ShowPersistentNotification("TEKAN & TAHAN 'E' UNTUK MENCARI UMPAN.");
            isInBaitZone = true; 
        }
        if (other.CompareTag("Tent")) { 
            // UIManager.instance.ShowPersistentNotification("TEKAN 'R' UNTUK ISTIRAHAT.");
            canRestAtTent = true; 
        }
        if (other.CompareTag("NPC")) {
            canTalkToNPC = true;
            // UIManager.instance.ShowPersistentNotification("TEKAN 'E' UNTUK BICARA.");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        UIManager.instance.HidePersistentNotification();
        currentPromptAssetScript = null;


        if (other.CompareTag("ShopZone")) {
            isInShopZone = false;
        }
        if (other.CompareTag("Dock")) { 
            isAtDock = false;
            // UIManager.instance.HidePersistentNotification();
        }
        if (other.CompareTag("BigBoatDock")) {
            isAtBigBoatDock = false;
            // UIManager.instance.HidePersistentNotification();
        }
        if (other.CompareTag("FishingZone")) { 
            isInFishingZone = false;
            // UIManager.instance.HidePersistentNotification();
        }
        if (other.CompareTag("BaitZone")) {
            isInBaitZone = false;
            // UIManager.instance.HidePersistentNotification();
        }
        if (other.CompareTag("Tent")) { 
            canRestAtTent = false;
            // UIManager.instance.HidePersistentNotification();
        }
        if (other.CompareTag("NPC")) {
            isInteractingWithNPC = false;
            canTalkToNPC = false;
            // UIManager.instance.HidePersistentNotification();
        }
    }

    // --- Fungsi WIN GAME ---
    private void WinGame()
    {
        // Tampilkan notifikasi kemenangan
        UIManager.instance.ShowPlayerBubble("LETS GOO!!");
        
        // Setelah delay, masuk ke scene Epilogue
        StartCoroutine(WinGameSequence());
    }

    // --- Coroutine untuk Win Game Sequence ---
    private IEnumerator WinGameSequence()
    {
        // Tunggu notifikasi ditampilkan
        yield return new WaitForSeconds(2f);
        
        // Fade to black selama 2 detik
        yield return StartCoroutine(FadeManager.FadeToBlack(2f));
        
        // Hapus semua save data (progress game sudah tamat)
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("--- SAVE DATA DIHANCURKAN ---");
        
        // Masuk ke scene Epilogue
        SceneManager.LoadScene("Epilogue");
    }

    public void SetUIState(bool isOpen)
    {
        isUIOpen = isOpen;

        if (isOpen)
        {
            Time.timeScale = 0f; // Bekukan Waktu (Animasi & Fisik berhenti)
            // Musik (AudioSource) biasanya tetap jalan meski timeScale 0, jadi aman!
        }
        else
        {
            Time.timeScale = 1f; // Jalankan Waktu lagi
        }
    }
}