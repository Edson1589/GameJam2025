using UnityEngine;

/// <summary>
/// Script temporal para debuggear problemas con las partes del cuerpo
/// Adjúntalo a RUBO_Player temporalmente
/// </summary>
public class BodyPartsDebugger : MonoBehaviour
{
    private PlayerController player;
    private CapsuleCollider capsule;

    void Start()
    {
        player = GetComponent<PlayerController>();
        capsule = GetComponent<CapsuleCollider>();
    }

    void Update()
    {
        // Ajuste manual de altura
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            Vector3 pos = transform.position;
            pos.y += 0.05f;
            transform.position = pos;
            Debug.Log($"Nueva altura Y: {pos.y}");
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            Vector3 pos = transform.position;
            pos.y -= 0.05f;
            transform.position = pos;
            Debug.Log($"Nueva altura Y: {pos.y}");
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log($"=== ALTURA ACTUAL: {transform.position.y} ===");
        }
    }

    void OnGUI()
    {
        GUIStyle style = new GUIStyle();
        style.fontSize = 20;
        style.normal.textColor = Color.white;

        GUI.Label(new Rect(10, 10, 500, 30), $"Has Torso: {player.hasTorso}", style);
        GUI.Label(new Rect(10, 40, 500, 30), $"Has Legs: {player.hasLegs}", style);
        GUI.Label(new Rect(10, 70, 500, 30), $"Has Arms: {player.hasArms}", style);
        GUI.Label(new Rect(10, 100, 500, 30), $"Position Y: {transform.position.y:F2}", style);
        GUI.Label(new Rect(10, 130, 500, 30), $"Collider Height: {capsule.height:F2}", style);
        GUI.Label(new Rect(10, 160, 500, 30), $"Collider Center: {capsule.center}", style);

        // Instrucciones
        style.fontSize = 16;
        style.normal.textColor = Color.yellow;
        GUI.Label(new Rect(10, 200, 500, 30), "Presiona D para debug detallado", style);
        GUI.Label(new Rect(10, 230, 500, 30), "Presiona R para resetear altura", style);
        GUI.Label(new Rect(10, 260, 500, 30), "Presiona T para forzar activar torso", style);
    }

    void PrintDebugInfo()
    {
        Debug.Log("========== DEBUG INFO ==========");
        Debug.Log($"Player Position: {transform.position}");
        Debug.Log($"Has Torso: {player.hasTorso}");
        Debug.Log($"Has Legs: {player.hasLegs}");
        Debug.Log($"Has Arms: {player.hasArms}");
        Debug.Log($"Capsule Height: {capsule.height}");
        Debug.Log($"Capsule Center: {capsule.center}");
        Debug.Log($"Capsule Radius: {capsule.radius}");

        // Verificar GameObjects
        Debug.Log($"HeadGO: {(player.headGO != null ? player.headGO.name + " (Active: " + player.headGO.activeSelf + ")" : "NULL")}");
        Debug.Log($"TorsoGroup: {(player.torsoGroup != null ? player.torsoGroup.name + " (Active: " + player.torsoGroup.activeSelf + ")" : "NULL")}");
        Debug.Log($"ArmsGroup: {(player.armsGroup != null ? player.armsGroup.name + " (Active: " + player.armsGroup.activeSelf + ")" : "NULL")}");
        Debug.Log($"LegsGroup: {(player.legsGroup != null ? player.legsGroup.name + " (Active: " + player.legsGroup.activeSelf + ")" : "NULL")}");
        Debug.Log("==============================");
    }

    void ResetHeightAndCollider()
    {
        // Resetear a configuración de cabeza sola
        Vector3 pos = transform.position;
        pos.y = 0.3f;
        transform.position = pos;

        capsule.height = 0.5f;
        capsule.center = new Vector3(0, 0.25f, 0);
        capsule.radius = 0.25f;

        Debug.Log("✅ Altura y collider reseteados a configuración de cabeza sola");
    }

    void ForceActivateTorso()
    {
        if (player.torsoGroup != null)
        {
            player.torsoGroup.SetActive(true);
            Debug.Log("✅ TorsoGroup activado manualmente");
        }
        else
        {
            Debug.LogError("❌ TorsoGroup es NULL! Verifica la asignación en PlayerController");
        }
    }

    void OnDrawGizmos()
    {
        // Dibujar el collider
        if (capsule != null)
        {
            Gizmos.color = Color.green;
            Vector3 center = transform.position + capsule.center;
            Gizmos.DrawWireSphere(center + Vector3.up * (capsule.height / 2 - capsule.radius), capsule.radius);
            Gizmos.DrawWireSphere(center + Vector3.down * (capsule.height / 2 - capsule.radius), capsule.radius);
        }

        // Dibujar posición Y
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * 2);
    }
}