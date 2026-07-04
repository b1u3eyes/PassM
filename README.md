# SecureVault

Manager de parole desktop pentru Windows, construit cu **WinUI 3** și **.NET 8**.

Parolele sunt stocate local într-un fișier criptat (`vault.enc`). Nu există server cloud — datele rămân pe calculatorul tău.

## Funcționalități

- **Seif criptat** — parolă master pentru deblocare
- **Gestionare intrări** — site, email, utilizator, parolă, URL, categorie
- **Căutare și filtre** — toate, favorite, parole slabe
- **Generator de parole** — lungime 4–128, litere/cifre/simboluri, caractere ambigue excluse
- **Evaluare putere parolă** — slabă / medie / puternică
- **Copiere în clipboard** — pentru câmpurile din intrare
- **Favorite** — marcare rapidă a intrărilor importante

## Securitate

| Aspect | Detaliu |
|--------|---------|
| Criptare | AES-256-GCM |
| Derivare cheie | PBKDF2-SHA256, 310.000 iterații |
| Format fișier | Header `SVLT` + versiune + salt + nonce + ciphertext + tag |
| Stocare | `%LocalAppData%\SecureVault\vault.enc` |
| Memorie | Zeroizare buffer-e sensibile după utilizare |

> **Atenție:** Dacă uiți parola master, **nu există recuperare**. Fă backup la `vault.enc`.

## Cerințe

- Windows 10 1809+ sau Windows 11
- Pentru **build din sursă:** [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) și workload Windows App SDK / WinUI 3

## Structura proiectului

```
SecureVault-src/
├── SecureVault/           # UI WinUI 3 (ViewModels, Views, Services)
└── SecureVault.Core/      # Logică: criptografie, vault, modele
```

## Build din sursă

```powershell
cd SecureVault-src
$Platform = $env:PROCESSOR_ARCHITECTURE   # AMD64 → x64

# Build
dotnet build SecureVault\SecureVault.csproj -c Debug -p:Platform=$Platform

# Rulare (debug)
dotnet run --project SecureVault\SecureVault.csproj -c Debug -p:Platform=$Platform
```

## Release portabil

```powershell
$Platform = $env:PROCESSOR_ARCHITECTURE

dotnet publish SecureVault\SecureVault.csproj `
  -c Release `
  -p:Platform=$Platform `
  -p:PublishProfile=win-x64
```

Output: `SecureVault\bin\Release\net8.0-windows10.0.26100.0\win-x64\publish\`

Copiază **întreg folderul** `publish` — `SecureVault.exe` are nevoie de DLL-urile din același director.

## Instalator MSIX

```powershell
dotnet publish SecureVault\SecureVault.csproj `
  -c Release `
  -p:Platform=x64 `
  -p:PublishProfile=msix-x64
```

Pachetul `.msix` apare în folderul Release. Este **nesemnat** — pentru instalare locală activează **Developer Mode** în Windows (Setări → Sistem → Pentru dezvoltatori).

## Utilizare

1. Pornește `SecureVault.exe` (portabil) sau instalează pachetul MSIX
2. La prima rulare: creează o parolă master puternică
3. La rulări ulterioare: introdu parola master pentru deblocare
4. Adaugă/editează/șterge intrări; modificările se salvează automat în seif

## Tehnologii

- WinUI 3 / Windows App SDK
- .NET 8
- CommunityToolkit.Mvvm
- System.Security.Cryptography (AES-GCM, PBKDF2)

## Licență

Distribuit sub licența [MIT](LICENSE).

## Contribuții

Pull request-urile sunt binevenite. Deschide un issue pentru bug-uri sau funcționalități noi.
