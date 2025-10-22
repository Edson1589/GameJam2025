using UnityEngine;

/// <summary>
/// Interfaz para objetos que pueden ser desactivados temporalmente por pulsos electromagn�ticos
/// </summary>
public interface IJammable
{
    /// <summary>
    /// Aplica el efecto de jamming por una duraci�n espec�fica
    /// </summary>
    /// <param name="duration">Duraci�n en segundos</param>
    void ApplyJam(float duration);

    /// <summary>
    /// Verifica si el objeto est� actualmente jammeado
    /// </summary>
    bool IsJammed();
}