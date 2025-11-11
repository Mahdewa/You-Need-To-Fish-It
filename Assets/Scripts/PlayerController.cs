using UnityEngine;
using System.Collections;

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

    // --- Variabel Sistem Memancing ---
    [Header("Sistem Memancing")]
    public float fishingWaitTime = 3.0f; 
    public float reelingDuration = 4.0f; 
    public float fishingEnergyCost = 10f;
    public float bridgeFishingSuccessChance = 0.3f; // 30%
    public float seaFishingSuccessChance = 0.3f; // 30%

    // --- Variabel Sistem Bait ---
    [Header("Sistem Bait")]
    public float baitSuccessChance = 0.3f; // 30%
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
    
    private enum FishingState { None, Casting, Waiting, Hooked, Reeling, Result }
    private FishingState currentFishingState = FishingState.None;


    void Start()
    {
        rbPlayer = GetComponent<Rigidbody2D>(); 
        playerSprite = GetComponent<SpriteRenderer>();
        playerCollider = GetComponent<Collider2D>();
    }

    void Update()
    {
        if (currentFishingState != FishingState.None) return;
        if (isBusy) return;
        // --- Sistem Kapal ---
        if (isAtBigBoatDock && Input.GetKeyDown(KeyCode.E)) {
            if (InventorySystem.instance.ownsBigBoat) {
                // Jika sudah, tamatkan game
                WinGame();
            }
            else {
                UIManager.instance.ShowNotification("YOU NEED TO BUY THE SHIP FIRST!");
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
                UIManager.instance.ShowNotification("YOU NEED TO BUY THE BOAT FIRST");
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
                SellAllRawFish();
            }
            // Tombol 'B' untuk BELI
            if (Input.GetKeyDown(KeyCode.B)) {
                if (!InventorySystem.instance.hasSmallBoat) { BuySmallBoat(); }
                else { BuyBigBoat(); }
            }
        }
    }

    void FixedUpdate()
    {
        // JANGAN bergerak jika sedang sibuk (mancing/gali)
        if (isBusy || currentFishingState != FishingState.None)
        {
            rbPlayer.linearVelocity = new Vector2(0, rbPlayer.linearVelocity.y); // Hentikan gerak X
            rbBoat.linearVelocity = Vector2.zero;
            return;
        }
        
        float moveInput = Input.GetAxis("Horizontal");
        
        if (isOnBoat)
        {
            rbBoat.linearVelocity = new Vector2(moveInput * moveSpeed, 0);
        }
        else
        {
            rbPlayer.linearVelocity = new Vector2(moveInput * moveSpeed, rbPlayer.linearVelocity.y);
            if (moveInput > 0) playerSprite.flipX = false;
            else if (moveInput < 0) playerSprite.flipX = true;
        }
    }

    // --- Fungsi BARU: Jual Ikan ---
    private void SellAllRawFish() {
        int fishToSell = InventorySystem.instance.rawFishCount;
        
        if (fishToSell > 0) {
            // 1. Hapus ikan dari inventory
            InventorySystem.instance.UseRawFish(fishToSell);
            
            // 2. Hitung total pendapatan
            int moneyEarned = fishToSell * rawFishPrice;
            
            // 3. Tambahkan uang
            InventorySystem.instance.AddMoney(moneyEarned);
            
            UIManager.instance.ShowPlayerBubble("MONEY!");
        } else {
            UIManager.instance.ShowPlayerBubble("NOTHING TO SELL");
        }
    }

    private void BuySmallBoat() {
        bool success = InventorySystem.instance.PurchaseItem(smallBoatPrice);
        if (success) {
            InventorySystem.instance.hasSmallBoat = true;
            UIManager.instance.ShowNotification("CONGRATULATIONS YOU HAVE BOUGHT THE BOAT! YOU CAN NOW USE IT TO FISH AT SEA!");
        }
    }

    // --- Fungsi BARU: Beli Kapal Besar (Objektif) ---
    private void BuyBigBoat() {
        // Cek dulu apakah sudah punya
        if (InventorySystem.instance.ownsBigBoat) {
            UIManager.instance.ShowNotification("YOU ALREADY OWN THE SHIP!");
            return;
        }
        
        // Panggil fungsi 'PurchaseItem' yang baru
        bool success = InventorySystem.instance.PurchaseItem(bigBoatPrice);
        if (success) {
            InventorySystem.instance.ownsBigBoat = true;
            UIManager.instance.ShowNotification("CONGRATULATIONS YOU HAVE BOUGHT THE SHIP! LETS TAKE A LOOK AND SEE WHAT'S GONNA HAPPEN!");
        }
    }

    // --- Coroutine BARU: Istirahat ---
    private IEnumerator RestSequence()
    {
        isBusy = true; // Player sibuk, tidak bisa bergerak
        // UIManager.instance.ShowNotification("Istirahat...");

        // (Di sini Anda bisa memicu animasi 'fade-to-black')
        
        yield return new WaitForSeconds(restTime); // Mensimulasikan waktu istirahat

        // Panggil fungsi BARU dari EnergySystem
        EnergySystem.instance.RestoreAllEnergy();
        
        // (Di sini Anda bisa memicu animasi 'fade-in')
        // UIManager.instance.ShowNotification("Segar kembali!");

        TimeSystem.instance.PassDay();

        isBusy = false; // Selesai, player bisa gerak lagi
    }

    // --- Coroutine MENCARI UMPAN (PERUBAHAN BESAR DI SINI) ---
    private IEnumerator StartBaitSequence() {
        isBusy = true; // Player sibuk, tidak bisa bergerak
        UIManager.instance.ShowPlayerBubble("!!??");


        float timer = 0f;

        // (Di sini Anda bisa memunculkan UI Progress Bar)
        // progressBar.fillAmount = 0;

        // --- Loop Selama Tombol Ditekan ---
        while (timer < baitHoldTime) {
            // Cek 1: Apakah tombol 'E' MASIH ditekan?
            if (!Input.GetKey(KeyCode.E)) {
                UIManager.instance.ShowNotification("SEARCH CANCELED");
                isBusy = false;
                // (Sembunyikan UI Progress Bar)
                yield break; // Hentikan coroutine
            }

            // Cek 2: Apakah player masih di zona bait?
            if (!isInBaitZone) {
                isBusy = false;
                // (Sembunyikan UI Progress Bar)
                yield break; // Hentikan coroutine
            }

            // Lanjutkan timer
            timer += Time.deltaTime;
            
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
        // (Sembunyikan UI Progress Bar)
    }

    // --- Coroutine Memancing ---
    private IEnumerator StartFishingSequence() {
        if (!InventorySystem.instance.HasBait(1)) {
            UIManager.instance.ShowPlayerBubble("NO BAIT");
            yield break;
        }
        if (!EnergySystem.instance.HasEnoughEnergy(fishingEnergyCost)) {
            UIManager.instance.ShowPlayerBubble("I'M TIRED");
            yield break;
        }
        
        // Kurangi resource
        InventorySystem.instance.UseBait(1);
        EnergySystem.instance.UseEnergy(fishingEnergyCost);
        
        currentFishingState = FishingState.Casting;
        UIManager.instance.ShowNotification("THROWING..."); 
        yield return new WaitForSeconds(0.5f); 

        currentFishingState = FishingState.Waiting;
        UIManager.instance.ShowNotification("WAITING...");
        yield return new WaitForSeconds(fishingWaitTime); 

        currentFishingState = FishingState.Hooked;
        UIManager.instance.ShowPlayerBubble("!");

        float hookTimer = 0f;
        bool playerReacted = false;
        while (hookTimer < reelingDuration) {
            if (Input.GetMouseButtonDown(0)) {
                playerReacted = true;
                break;
            }
            hookTimer += Time.deltaTime;
            yield return null; 
        }
        
        if (!playerReacted) {
            UIManager.instance.ShowNotification("FISH ESCAPED");
            currentFishingState = FishingState.None; 
            yield break; 
        }

        currentFishingState = FishingState.Reeling;
        UIManager.instance.ShowNotification("REELING!");
        yield return new WaitForSeconds(reelingDuration - hookTimer);

        UIManager.instance.ShowNotification("??!!!");
        currentFishingState = FishingState.Result;
        
        float currentChance;
        if (isOnBoat) {
            // Player ada di perahu -> pakai chance LAUT
            currentChance = seaFishingSuccessChance;
            UIManager.instance.ShowNotification($"FISHING AT SEA... (CHANCE: {currentChance * 100}%)");
        } else {
            // Player di darat (jembatan) -> pakai chance JEMBATAN
            currentChance = bridgeFishingSuccessChance;
            UIManager.instance.ShowNotification($"MEMANCING AT BRIDGE... (CHANCE: {currentChance * 100}%)");
        }

        // 2. Gunakan 'currentChance' untuk menentukan hasil
        if (Random.value <= currentChance) { 
            UIManager.instance.ShowPlayerBubble("GOT A FISH!");
            InventorySystem.instance.AddRawFish(1); 
        } else { 
            UIManager.instance.ShowPlayerBubble("HUFT, UNLUCKY!"); 
        }
        yield return new WaitForSeconds(1.0f); 

        // UIManager.instance.ShowNotification("SIAP MEMANCING LAGI.");
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
        }
        else
        {
            rbPlayer.isKinematic = false; 
            playerCollider.isTrigger = false; 
            this.transform.SetParent(null); 
            this.transform.position = playerLandPosition.position; 
            rbBoat.transform.position = boatStartPosition.position;
            rbBoat.linearVelocity = Vector2.zero;
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
        UIManager.instance.ShowNotification("SELAMAT! KAMU TELAH MENYELESAIKAN PERJALANANMU!");
        
        // Disable player input (freeze game)
        Time.timeScale = 0f;
        
        // Simpan data game (opsional)
        if (SaveSystem.instance != null)
        {
            SaveSystem.instance.SaveGameData();
        }
        
        // Setelah delay, reload ke scene awal atau tampilkan menu
        StartCoroutine(WinGameSequence());
    }

    // --- Coroutine untuk Win Game Sequence ---
    private IEnumerator WinGameSequence()
    {
        yield return new WaitForSecondsRealtime(3f); // Tunggu 3 detik (real time, bukan game time)
        
        // Kembalikan time scale ke normal
        Time.timeScale = 1f;
        
        // Load scene menu atau scene lain sesuai kebutuhan
        // UnityEngine.SceneManagement.SceneManager.LoadScene("MenuScene");
        // Untuk saat ini, cukup tampilkan pesan
    }
}