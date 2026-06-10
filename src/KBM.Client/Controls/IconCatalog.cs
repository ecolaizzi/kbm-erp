namespace KBM.Client.Controls;

/// <summary>
/// Catalogo centralizzato delle icone (Segoe MDL2 Assets) per azioni ricorrenti del gestionale.
/// Usare SEMPRE queste costanti per garantire coerenza visiva tra tutte le schermate (stile NTS Business Cube).
/// </summary>
public static class IconCatalog
{
    // CRUD / record
    public const string New = "\uE710";        // +
    public const string Open = "\uE8E5";       // apri cartella
    public const string Save = "\uE73E";       // check
    public const string Edit = "\uE70F";       // matita
    public const string Duplicate = "\uE8C8";  // copia
    public const string Delete = "\uE74D";     // cestino
    public const string Undo = "\uE7A7";       // annulla
    public const string Cancel = "\uE711";     // X

    // Stato
    public const string Enable = "\uE73E";     // abilita (check)
    public const string Disable = "\uE738";    // disabilita (block/remove)
    public const string Lock = "\uE72E";

    // Liste / dati
    public const string Refresh = "\uE72C";
    public const string Filter = "\uE71C";
    public const string Search = "\uE721";
    public const string Columns = "\uE71D";
    public const string Sort = "\uE8CB";
    public const string Print = "\uE749";
    public const string Export = "\uE74E";     // salva/esporta
    public const string Import = "\uE8B5";
    public const string Excel = "\uE9F9";

    // Navigazione record
    public const string First = "\uE892";
    public const string Prev = "\uE76B";
    public const string Next = "\uE76C";
    public const string Last = "\uE893";

    // Pannelli / contesto
    public const string Detail = "\uE890";     // wings
    public const string Settings = "\uE713";
    public const string Menu = "\uE700";
    public const string More = "\uE712";
    public const string Help = "\uE897";
    public const string Exit = "\uE7E8";

    // Entità anagrafiche
    public const string Customer = "\uE77B";
    public const string Supplier = "\uE7EE";
    public const string Item = "\uE7B8";
    public const string Company = "\uE825";
    public const string User = "\uE716";
    public const string Address = "\uE707";
    public const string Contact = "\uE77B";
    public const string Bank = "\uE825";
    public const string Document = "\uE8A5";
}
