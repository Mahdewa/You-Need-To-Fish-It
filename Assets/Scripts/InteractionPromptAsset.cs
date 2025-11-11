using UnityEngine;
using UnityEngine.UI; // Kita butuh ini untuk tipe data 'Sprite'

public class InteractionPromptAsset : MonoBehaviour
{
    [Header("Aset Prompt untuk Objek ini")]
    
    // Prompt utama (misal: [E] TALK)
    public Sprite defaultPrompt; 
    
    // Khusus untuk NPC:
    // Prompt saat player belum punya perahu (misal: [J] JUAL | [B] BELI PERAHU)
    public Sprite boatPurchasePrompt;
    
    // Prompt saat player sudah punya perahu (misal: [J] JUAL | [B] BELI KAPAL)
    public Sprite shipPurchasePrompt; 
}