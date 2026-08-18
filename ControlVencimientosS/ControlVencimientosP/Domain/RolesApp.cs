namespace ControlVencimientosP.Domain;

/// <summary>Tres roles y ni uno mas. Ver la seccion "lo que NO es" del plan.</summary>
public static class RolesApp
{
    public const string Admin = "Admin";
    public const string Editor = "Editor";
    public const string Lector = "Lector";

    public static readonly string[] Todos = [Admin, Editor, Lector];
}
