De Fluitende Fietser - samengevoegde WPF-applicatie

Open DeFluitendeFietser.sln in Visual Studio 2026. Stel DeFluitendeFietser in als Startup Project en start met F5.

De app heeft een startscherm met knoppen naar vier vensters: Bestelpagina, Betaalsysteem, Rekenmachine en Klok. De schermindelingen zijn overgenomen uit de aangeleverde projecten; de vensterklassen zijn aangepast om ze in één WPF-project te kunnen gebruiken.

Benodigd: .NET 10 SDK en Windows met WPF-ondersteuning. De CSV en afbeeldingen zijn inbegrepen.

Controle: de XAML is aangepast zodat deze niet langer verwijst naar de oude, afzonderlijke namespaces. Deze omgeving heeft geen .NET SDK en kan de WPF-app daarom niet compileren of uitvoeren. Voer in Visual Studio Build > Rebuild Solution uit en test daarna elk venster. Als er nog compilerfouten verschijnen, stuur dan de volledige Error List.
