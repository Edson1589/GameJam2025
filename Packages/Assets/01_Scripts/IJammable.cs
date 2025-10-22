using UnityEngine;

/// <summary>
/// Interfaz para objetos que pueden ser desactivados temporalmente por pulsos electromagnéticos
/// </summary>
public interface IJammable
{
    /// <summary>
    /// Aplica el efecto de jamming por una duración específica
    /// </summary>
    /// <param name="duration">Duración en segundos</param>
    void ApplyJam(float duration);

    /// <summary>
    /// Verifica si el objeto está actualmente jammeado
    /// </summary>
    bool IsJammed();
}