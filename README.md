## Mały Quizizz — Dokumentacja techniczna

### 1. Stack technologiczny

**Backend — ASP.NET Core**

ASP.NET Core to framework do budowy aplikacji webowych i API REST. W projekcie odpowiada za:
- **Routing** — mapowanie adresów URL na kontrolery (`QuizController`, `SubmissionController`, `FullQuizController`).
- **Walidację** — atrybuty `[Required]`, `[Range]`, `[Length]` na DTO sprawdzają dane wejściowe przed wejściem do logiki.
- **Dependency Injection** — kontrolery i serwisy otrzymują zależności przez konstruktor, bez ręcznego tworzenia obiektów.
- **Autoryzację** — `[Authorize]` na kontrolerach (obecnie odłączone w wersji demonstracyjnej, patrz sekcja 6).

**MongoDB Driver**

Oficjalny sterownik C# do MongoDB. Mapuje dokumenty BSON na klasy C# (`Quiz`, `Question`, `Answer`, `User`) i pozwala na elastyczne modelowanie danych. W tym projekcie quiz jest **jednym dokumentem** z zagnieżdżonymi pytaniami i odpowiedziami — nie ma relacji jeden-do-wielu w osobnych kolekcjach.

**MongoDB Atlas**

Zarządzana chmura bazy danych. Hostuje kolekcje `quizzes`, `users` i `submissions`. Aplikacja łączy się z nią przez connection string w `appsettings.json`.

**Frontend — Blazor (Interactive Server)**

Blazor to framework UI od Microsoftu, który pozwala pisać interaktywne aplikacje webowe w C# zamiast JavaScriptu. W trybie **Interactive Server**:
- Komponenty renderują się po stronie serwera.
- Przeglądarka utrzymuje stałe połączenie z serwerem przez **SignalR**.
- Kliknięcia i zmiany stanu są przesyłane do serwera, który re-renderuje komponent i odsyła różnicę do przeglądarki.
- Nie ma pełnego przeładowania strony — UI reaguje natychmiast.

---

### 2. Pipeline danych — jak dane płyną przez aplikację

Zrozumienie przepływu danych jest kluczowe, ponieważ **każda strona w aplikacji działa według tego samego schematu**.

#### Krok 1: Komponent Blazor inicjuje żądanie

Każda strona (np. `QuizList.razor`) dziedziczy po `ComponentBase` i nadpisuje metodę cyklu życia `OnInitializedAsync`:

```csharp
protected override async Task OnInitializedAsync()
{
    quizzes = await Api.GetQuizzesAsync();
}
```

`OnInitializedAsync` uruchamia się **raz**, gdy komponent pojawia się na stronie. To jest moment, w którym komponent prosi o dane.

#### Krok 2: `QuizApiService` wysyła żądanie HTTP

`QuizApiService` to **typed wrapper** wokół `HttpClient`. Komponent nie zna adresów URL ani formatu JSON — wywołuje metodę, a klient tłumaczy ją na żądanie HTTP:

```csharp
public Task<List<QuizDTO>?> GetQuizzesAsync() =>
    http.GetFromJsonAsync<List<QuizDTO>>("quiz");
```

`http` pochodzi z `IHttpClientFactory` (nazwany klient `"QuizApiClient"`), który:
- Ustawia `BaseAddress` na adres API (`http://localhost:5212/api/`).
- Ustawia timeout (30 s).
- **Automatycznie dokleja nagłówek `Authorization: Bearer <token>`** przez `AuthHeaderHandler` (gdy token jest dostępny — patrz sekcja 5).

#### Krok 3: API odbiera żądanie

Żądanie trafia do `QuizController` w projekcie `quizApi`:

```csharp
[HttpGet]
public async Task<IActionResult> GetQuizzes([FromQuery] int? limit, [FromQuery] int? skip)
{
    List<QuizDTO> quizzes = await _quizService.GetQuizzes(limit, skip);
    return Ok(quizzes);
}
```

Kontroler:
1. Odbiera parametry zapytania (`limit`, `skip`).
2. Deleguje do `IQuizService` — warstwy logiki biznesowej.
3. Zwraca `Ok(dto)` — odpowiedź 200 z listą DTO.

#### Krok 4: Serwis pyta MongoDB

`QuizService` używa `IMongoCollection<Quiz>` (wstrzyknięte przez DI) do zapytania bazy:

```csharp
var quizzes = await _collection
    .Find(FilterDefinition<Quiz>.Empty)
    .Skip(skip)
    .Limit(limit)
    .ToListAsync();

return _mapper.Map<List<QuizDTO>>(quizzes);
```

Tutaj dzieje się **mapowanie modelu domenowego na DTO** — `Quiz` (model MongoDB) → `QuizDTO` (kontrakt API). AutoMapper robi to automatycznie na podstawie profilu (`AutoMapperProfile`).

#### Krok 5: Odpowiedź wraca do komponentu

Blazor deserializuje JSON do `List<QuizDTO>` i przypisuje do pola `quizzes`. Gdy `OnInitializedAsync` się kończy, komponent **automatycznie się re-renderuje**, a `@foreach` w markupie wyświetla dane.

#### Podsumowanie pipeline'u

```
Komponent Blazor
    │  (wywołuje metodę)
    ▼
QuizApiClient (HttpClient)
    │  (żądanie HTTP + Bearer token)
    ▼
Kontroler API (QuizController)
    │  (deleguje)
    ▼
Serwis (QuizService)
    │  (zapytanie MongoDB)
    ▼
MongoDB (kolekcja quizzes)
    │  (wynik)
    ▼
AutoMapper (Quiz → QuizDTO)
    │  (mapowanie)
    ▼
Kontroler API → JSON
    │  (odpowiedź HTTP)
    ▼
QuizApiClient → List<QuizDTO>
    │  (deserializacja)
    ▼
Komponent Blazor → re-render
```

Ten sam schemat obowiązuje dla **każdej** operacji w aplikacji: pobieranie quizu, tworzenie quizu, dodawanie pytania, usuwanie. Różnice są tylko w metodzie HTTP (`GET`, `POST`, `PATCH`, `DELETE`) i kształcie DTO.

---

### 3. `quizShared` — wspólna biblioteka kontraktów

`quizShared` to **osobny projekt klas** (class library) referencjonowany przez **oba** projekty: `quizApi` i `quizUi`.

#### Co zawiera

```
quizShared/
├── UserType.cs              (enum: Student, Teacher, Admin)
├── Subject.cs               (enum: przedmiot quizu)
├── QuestionType.cs          (enum: singleChoice, multipleChoice)
└── DTOs/
    ├── QuizDTO.cs
    ├── QuestionDTO.cs
    ├── AnswerDTO.cs
    ├── CreateQuizDTO.cs
    ├── UpdateQuizDTO.cs
    ├── FullQuizDTO.cs
    ├── SubmissionDTO.cs
    ├── UserDTO.cs
    ├── RegisterUserDTO.cs
    ├── LoginDTO.cs
    └── ...
```

#### Dlaczego istnieje

Bez `quizShared` oba projekty musiałyby **duplikować** definicje DTO. To prowadzi do problemu: gdy API zmienia pole w `QuizDTO`, UI nadal używa starej wersji i deserializacja cicho gubi dane (albo wybucha `JsonException`).

Wspólna biblioteka sprawia, że:
- **Kompilator pilnuje zgodności** — jeśli API zmieni `QuizDTO.Title` na `QuizDTO.Name`, projekt UI nie skompiluje się, dopóki nie zaktualizujesz strony.
- **Nie ma dryfu** — jedno źródło prawdy dla kształtu danych.
- **Refaktor jest bezpieczny** — zmiana nazwy pola to jedno `Ctrl+R`, nie trzy oddzielne zmiany w dwóch projektach.

#### Czego `quizShared` **nie** zawiera

- **Modeli domenowych** (`Quiz`, `Question`, `Answer`, `User`) — te żyją tylko w `quizApi/Models/`. UI nie potrzebuje ich znać; widzi wyłącznie DTO.
- **Logiki biznesowej** — `quizShared` to czyste kontrakty, bez metod (poza enumami).
- **Zależności** — `quizShared` nie referencjonuje niczego. Jest na samym dole grafu zależności.

#### Graf zależności

```
quizShared  (bez zależności)
    ▲
    │
    ├── quizApi  (referencuje quizShared)
    └── quizUi   (referencuje quizShared)
```

`quizApi` i `quizUi` **nie znają się nawzajem** — nie ma referencji projektowej między nimi. Komunikują się wyłącznie przez HTTP i wspólne DTO.

---

### 4. Wzorzec strony — jak zbudować dowolną stronę

Ten rozdział opisuje **szablon**, którego trzyma się każda strona w aplikacji. Jeśli profesor chce odtworzyć dowolną stronę, wystarczy zrozumieć ten szablon — reszta to powtórka.

#### 4.1 Trzy stany każdej strony

Każda strona ładująca dane z API ma **trzy stany**, które muszą być obsłużone w markupie:

```razor
@if (error is not null)
{
    <p class="error">@error</p>
    <button @onclick="LoadAsync">Spróbuj ponownie</button>
}
else if (data is null)
{
    <p>Ładowanie...</p>
}
else
{
    @* właściwy UI *@
}
```

- **`error`** — żądanie się nie powiodło (API niedostępne, 404, 500).
- **`data is null`** — żądanie w toku, dane jeszcze nie dotarły.
- **`else`** — dane są, można renderować.

Pominięcie któregokolwiek stanu prowadzi do bugów: brak `error` → użytkownik widzi wieczny spinner; brak `null` → `NullReferenceException` przy pierwszym renderze.

#### 4.2 Ładowanie danych

```csharp
private List<QuizDTO>? data;
private string? error;

protected override Task OnInitializedAsync() => LoadAsync();

private async Task LoadAsync()
{
    error = null;
    data = null;

    try
    {
        data = await Api.GetQuizzesAsync();
    }
    catch (Exception ex)
    {
        Logger.LogError(ex, "Failed to load");
        error = "Nie udało się załadować danych.";
    }
}
```

Wzorzec: **wyzeruj stan → spróbuj → zapisz wynik albo błąd**. `LoadAsync` jest wywoływane zarówno z `OnInitializedAsync` (pierwsze ładowanie), jak i z przycisku "Spróbuj ponownie" (retry).

#### 4.3 Mutacje (create / update / delete)

Dla operacji zapisu wzorzec jest podobny, ale z flagą `isBusy` zamiast `data is null`:

```csharp
private bool isBusy;

private async Task SaveAsync()
{
    if (isBusy) return;   // zapobiega podwójnemu kliknięciu
    isBusy = true;
    error = null;

    try
    {
        await Api.UpdateQuizAsync(id, model);
        // sukces — odśwież dane albo nawiguj
    }
    catch (Exception ex)
    {
        Logger.LogError(ex, "Failed to save");
        error = "Nie udało się zapisać.";
    }
    finally
    {
        isBusy = false;   // zawsze zdejmij blokadę
    }
}
```

`isBusy` pełni dwie role: **blokuje przycisk** (`disabled="@isBusy"`) i **zapobiega wyścigowi**, gdy użytkownik kliknie dwa razy.

#### 4.4 Nawigacja między stronami

Trzy mechanizmy:

| Mechanizm | Kiedy używać |
|---|---|
| `<a href="/quiz/@id">` | Zwykły link — najprostszy, działa z "otwórz w nowej karcie" |
| `Nav.NavigateTo("/quizzes")` | Po akcji (np. po usunięciu quizu) — programowa nawigacja |
| `QuizStateService` | Przekazanie obiektu między stronami bez ponownego fetchu |

#### 4.5 Przekazywanie danych między stronami

Blazor **nie ma** wbudowanego mechanizmu przekazywania obiektów przez URL. Rozwiązanie: **scoped service** (`QuizStateService`):

```csharp
public class QuizStateService
{
    private QuizDTO? _current;
    public void SetQuiz(QuizDTO quiz) => _current = quiz;
    public QuizDTO? ConsumeQuiz()
    {
        var tmp = _current;
        _current = null;
        return tmp;
    }
}
```

Strona źródłowa woła `SetQuiz(quiz)` przed nawigacją; strona docelowa woła `ConsumeQuiz()` w `OnParametersSetAsync`. Wzorzec "consume" (zamiast "get") jest istotny — czyści stan po odczycie, żeby kolejna nawigacja nie dostała starego obiektu.

#### 4.6 Lista kontrolna dla nowej strony

1. `@page "/route"` na górze.
2. `@inject QuizApiClient Api` (i `ILogger<T>` jeśli logujesz).
3. Pola: `data`, `error`, `isBusy`.
4. `OnInitializedAsync` → `LoadAsync()`.
5. Markup: trzy stany (`error` / `null` / `else`).
6. Mutacje: `isBusy` + `try/catch/finally`.

To wszystko. Każda strona w projekcie to ten szablon z innym DTO i innym routem.

---

### 5. Uwierzytelnianie (stan obecny)

**Zaimplementowane:**
- Model `User` + kolekcja `users` w MongoDB.
- `AuthController` — `POST auth/register`, `POST auth/login`.
- Generowanie i walidacja JWT (`JwtOptions`, `AddJwtBearer`).
- `UserService` — hash haseł BCrypt (work factor 12).
- Strony `Login.razor` i `Register.razor` w UI.

**Niedokończone:**
- Propagacja tokenu z obwodu Blazor Server do wychodzących żądań HTTP. Problem: `IHttpClientFactory` tworzy `DelegatingHandler` w **innym scope DI** niż obwód Blazor, więc handler nie widzi `TokenService` ustawionego przez stronę logowania.
- Planowane rozwiązanie: `CircuitHandler` + `AsyncLocal<IServiceProvider>` (wzorzec z dokumentacji Microsoftu).

**Stan demonstracyjny:** `FullQuizController` jest **publicznie dostępny** (bez `[Authorize]`), żeby demo działało bez logowania. W produkcji ten kontroler byłby chroniony `[Authorize(Roles = "Teacher")]`.

---

### 6. Praca przyszła

1. **Dokończenie propagacji tokenu** — `CircuitHandler` + `AuthHeaderHandler`, `[Authorize]` na `FullQuizController`, `AuthorizeRouteView` na `/quiz/{id}/edit`.
2. **Wdrożenie chmurowe** — Oracle Cloud (lub inny provider) zamiast `localhost`. Wymaga: konteneryzacji API + UI, konfiguracji MongoDB Atlas (już jest w chmurze), HTTPS, zmiennych środowiskowych.
3. **Zarządzanie użytkownikami** — panel admina, reset hasła, role.
4. **Testy** — jednostkowe dla serwisów, integracyjne dla kontrolerów.

---

### 7. Uruchomienie projektu

```bash
# Terminal 1 — API
cd quizApi
dotnet run

# Terminal 2 — UI
cd quizUi
dotnet run
```

Wymagane:
- .NET 8+ SDK.
- Connection string do MongoDB Atlas w zmiennej środowiska ATLAS_URI.
- Porty: API `5212`, UI `5064` (lub zgodnie z `launchSettings.json`).
