// =======================================================
// README: Inventory System
// Aflevering for uge 6 – Industriel Programmering
// =======================================================

// Projekt:
// Dette program er et simpelt Inventory System skrevet i C# med Avalonia GUI
// Programmet viser et lager, ordrer i kø og ordrer som er blevet behandlet.
// Der er to DataGrids i vinduet,  et til "Queued Orders" og et til "Processed Orders".
// Når brugeren trykker på knappen process next order, flyttes næste ordre fra køen over i listen med færdigbehandlede ordrer.
// Omsætningen opdateres automatisk.

// Opbygning:
// GUI lavet i avalonia XAML.
// Logik placeret i viewmodel med mvvm.
// Domæneklasser er som angivet (Item, Order, Inventory osv.) ligger i models mappe.
// MainWindow.xaml viser data med bindinger til ViewModel.
// Program.cs starter appen via Avalonia’s desktop-lifetime.

// AI-brug:
// Denne aflevering er udviklet med hjælp fra 
// kilde: ChatGPT (OpenAI, 2025).
// ChatGPT har været brugt som feedback og kodeassistent under arbejdet.
// Konkret har jeg brugt AI til at:
// Generere et grundskelet for Avalonia GUI og MVVM-struktur ud fra mine udarbejdede aktivities og noter fra timen.
// Hjælpe med at rette build fejl og forstå bindinger i XAML, samt sparring med noter og pensum for besvarelse af aflevering.
// Den har derudover givet forslag til kommentarer og forenkling af viewmodel koden.
// Brugte også til read me og class diagram skelet, hvor jeg har skrevet om til eget sprog.
// Jeg har selv skrevet og tilpasset al kode, gennemgået logikken,
// og indsat mine egne danske kommentarer for at vise forståelse af pensum.
// Jeg tager fuldt ansvar for den endelige kode, struktur og rapport.
// Jeg er ansvarlig for den afleverede løsning.

//Filervedlagt
// Screen cap
// Flowdiagram
// Class diagram


// =======================================================
