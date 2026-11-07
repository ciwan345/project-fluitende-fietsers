DE FLUITENDE FIETSER - WPF KASSASYSTEEM

Dit is een oefenproject voor C# en WPF.

PROJECTSTRUCTUUR
- App.xaml en App.xaml.cs: starten de applicatie.
- MainWindow.xaml: de indeling van het hoofdvenster.
- MainWindow.xaml.cs: knoppen en functies van Bestellen, Betalen, Rekenmachine en Klok.
- Models/Product.cs: gegevens van een product.
- Models/OrderItem.cs: een productregel in de bestelling.
- Models/CustomerRecord.cs: gegevens van een afgeronde klantbetaling.
- Views/CustomerWindow.xaml: het venster met het klantenoverzicht.
- Views/CustomerWindow.xaml.cs: gegevens tonen in het klantenvenster.
- Helpers/CalculatorParser.cs: eigen rekenfuncties voor de rekenmachine.
- prijzen_fietsen_verzekeringen_services.csv: producten en prijzen.

ONGEBRUIKTE BESTANDEN
De map Images is verwijderd. De afbeeldingen werden niet gebruikt in de XAML of C# code.

STARTEN IN VISUAL STUDIO
1. Pak het ZIP-bestand uit.
2. Open DeFluitendeFietser.sln in Visual Studio.
3. Wacht tot NuGet en de projectbestanden geladen zijn.
4. Druk op F5 om het programma te starten.

BELANGRIJK
- De applicatie gebruikt WPF en C#.
- De rekenmachine gebruikt decimalen en eigen methodes voor de berekening.
- Klanten worden na een geslaagde betaling opgeslagen in klanten_historie.csv.
- Dit is een eenvoudige kassasimulatie. Er is geen echte pinterminal gekoppeld.

Tip: lees de methodes in MainWindow.xaml.cs en probeer zelf kleine wijzigingen te maken.
