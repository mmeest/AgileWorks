# Pöördumiste rakendus

* [Ülesande kirjeldus](#ülesande-kirjeldus)
* [Andmebaas](#andmebaas)
* [Ühendusstring Andmebaasiga](#ühendusstring)
* [Lisatud paketid](#lisatud-paketid)
* [DBContext laiendus](#dbcontext-laiendus)
* [Pöördumiste mudel](#pöördumiste-mudel)
* [xUnit testid](#xunit-testid)
* [Rakenduse pealehekülg](#rakenduse-pealehekülg)
* [Pöördumise lisamine](#pöördumise-lisamine)
* [Lahendatuks märkimine](#lahendatuks-märkimine)

## Ülesande kirjeldus

Teostada lihtne veebirakendus, mis võimaldaks hallata kasutajatoele saadetud pöördumisi.

Lihtsustatud süsteemi funktisonaalsus:

* Kasutaja saab sisestada pöördumise.
* Pöördumisel peab olema kirjeldus, sisestamise aeg, lahendamise tähtaeg. Sisestamise ajaks märgitakse pöördumise sisestamise aeg, teised kohustuslikud väljad täidab kasutaja.
* Kasutajale kuvatakse aktiivsed pöördumised koos kõigi väljadega nimekirjas sorteeritult kahanevalt lahendamise tähtaja järgi.
* Pöördumised, mille lahendamise tähtajani on jäänud vähem kui 1 tund või mis on juba ületanud lahendamise tähtaja, kuvatakse nimekirjas punasena.
* Kasutaja saab nimekirjas pöördumisi lahendatuks märkida, mis kaotab pöördumise nimekirjast.

Nõuded:

* Testtöö serveripoolne osa lahendada .NET platvormil.
* Võib eeldada modernse brauseri olemasolu (HTML5 jne).
* Andmeid ei ole vaja andmebaasi salvestada, võib hoida neid ka mälus.
* Boonuspunktid, kui lahendust katavad ka testid.

## Andmebaas

Andmebaas koopia on talletatud 'DATABASE' kataloogi.
SQL andmebaas on loodud ja lisatud 5 sissekannet:

```
CREATE TABLE requests (
		Id INT NOT NULL PRIMARY KEY IDENTITY,
		Content VARCHAR (300) NOT NULL,
		CreatedAt DATETIME NOT NULL,
		Deadline DATETIME NOT NULL,
		Resolved BIT NOT NULL
);

INSERT INTO requests (Content, CreatedAt, Deadline, Resolved)
VALUES
('Esimene pöördumine', '2024-03-10 12:00:12', '2024-03-15 12:00:12', 0),
('Teine pöördumine', '2024-03-10 12:00:12', '2024-03-15 12:00:12', 0),
('Kolmas pöördumine', '2024-02-10 12:00:12', '2024-02-15 12:00:12', 1),
('Neljas pöördumine', '2025-03-10 12:00:12', '2025-03-15 12:00:12', 0),
('Viies pöördumine', '2023-03-10 12:00:12', '2023-03-15 12:00:12', 0);
```

Resolved(bool) väli on loodud pöördumiste lahendatuks märkimiseks. Neljas pöördumine on juba lahendatuks märgitud, seega seda ei kuvata rakenduse vaates.

## Ühendusstring

Andmebaasi ühendusstring on talletatud 'Program.cs' failis järgnevalt:

```
builder.Services.AddDbContext<RequestsContext>(options =>
{
    options.UseSqlServer("Server=DESKTOP-QGORNM4\\SQLEXPRESS;Database=Requests_DB;Trusted_Connection=True;TrustServerCertificate=True;");
});
```

## Lisatud paketid

Rakendusele lisatud paketid andmebaasi kasutamiseks ning kujunduseks:

* Microsoft.EntityFrameworkCore				
* Microsoft.EntityFrameworkCore.SqlServer
* Microsoft.EntityFrameworkCore.Tools
* Bootstrap

## DbContext laiendus

'RequestsContext.cs'

```
using Azure.Core;
using Microsoft.EntityFrameworkCore;

namespace Requests_App.Models
{
    public class RequestsContext : DbContext
    {
        public DbSet<Request> Requests { get; set; }
        public RequestsContext(DbContextOptions options) : base(options)
        {

        }
    }
}
```


## Pöördumiste mudel

'Requests.cs'

```
using System.ComponentModel.DataAnnotations;

namespace Requests_App.Models
{
    public class Request
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Content is required")]
        [StringLength(255, MinimumLength = 1, ErrorMessage = "Content length must be between 1 and 255 characters")]
        public string Content { get; set; }
        [Required]
        public DateTime CreatedAt { get; set; }
        [Required]
        public DateTime Deadline { get; set; }
        [Required]
        public bool Resolved { get; set; }
    }
}

```

## xUnit testid

xUnit testide tarvis on lisatud järgnevad paketid:

```
xunit		
xunit.runner.visualstudio
Microsoft.NET.Test.Sdk		
Microsoft.EntityFrameworkCore.InMemory
Microsoft.AspNetCore.Mvc.Testing
```

Program.cs lisada juurdepääsuks:

```
public partial class Program
{

}
```

Ning xUnit Dependencies ühendatud Requests_App programmiga.

Testid asuvad:

```
Requests_App.UnitTests/Tests.cs
```


## Rakenduse pealehekülg

Pealehekülje route' on muudetud järgnevalt:

```
Requests/Index
```

Antud indeks leheküljel kuvatakse tabelikuva nelja andmebaasis oleva pöördumisega mis ei ole lahendatuks märgitud.

```
_context.Requests.Where(r => r.Resolved == false)
```

Sissekanded kuvatud tähtajaliselt kahanevas järjekorras.  

```
.OrderByDescending(r => r.Deadline)
```

Punaselt kuvatakse pöördumisi mille tähtaeg on möödas või mille tähtajani vähem kui 1 h.

```
var isOverdue = DateTime.Now > request.Deadline;
var isWithinOneHour = request.Deadline.Subtract(DateTime.Now).TotalHours < 1;
<tr style="@(isOverdue || isWithinOneHour ? "color:red;" : "")">
```

Kasutaja saab lisada uue pöördumise 'Lisa pöördumine' nupule klikkides.
Tabelis on iga pöördumise rea lõpus nupp 'Muuda staatust' mille abil saab vastava pöördumise staatus muuta lahendatuks.

## Pöördumise lisamine

Selleks, et lisada uut pöördumist tuleb klikkida nupul 'Lisa pöördumine', 
mispeale avaneb 'Requests/Create' vaade. Pöörudmise lisamiseks tuleb lisada pöördumise tekst
ja tähtaja kuupäev/kellaaeg ning klikkida nupule 'Lisa'. Seepeale ilmub lisatud pöördumine tabelisse.

## Lahendatuks märkimine

Selleks, et tabelis kuvatavat pöördumist lahendatuks märkida tuleb klikkida vastava pöördumise
rea lõpus olevat nuppu 'Muuda staatust'. Seepeale avaneb uus vaade milles tuleb lisada linnuke ning
klikkida nuppu 'Salvesta'

Lahendatuks märgitud pöördumised jäävad küll andmebaasi, kuid neid ei kuvata enam 'Lahendamata pöördumiste' 
tabelis.
