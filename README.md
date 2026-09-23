# singstar-projekt

Group project in TFYA65 Ljudfysik. A singstar-like game with pitch detection and score based on accuracy.

## Överenskommelse: mikrofon → pitchdetektor

Mikrofondelen hämtar ljud och skickar följande till pitchdetektorn:

- **Ljud:** en `float[]` med monosamplingar i tidsordning, från äldst till nyast.
- **Blockstorlek:** till att börja med 2048 samplingar per analys.
- **Samplingsfrekvens:** ljudets faktiska samplingsfrekvens i Hz skickas med tillsammans med blocket.

Pitchdetektorn returnerar:

- **Grundfrekvens i Hz.**
- **Ton hittad:** `true` eller `false`. Frekvensvärdet används bara när en ton har hittats.

Mikrofondelen levererar samplingar i tidsdomänen, utan FFT. Pitchdetektorn ansvarar för analysen. Första implementationen använder autokorrelation.

Delarna utvecklas separat: pitchdetektorn testas först med en skapad sinusvåg med känd frekvens. Därefter kopplar vi ihop den med mikrofonen.

## Målton och visuell feedback

Bygg en separat testscen som visar vilken ton spelaren ska sjunga och hur spelarens tonhöjd förhåller sig till den.

Första versionen:

- Använd en fast målton, exempelvis 220 Hz, som visas som ett horisontellt streck.
- Visa spelarens tonhöjd med en markör och indikera för högt, för lågt eller nära måltonen.
- Styr spelarens tonhöjd med en slider i Hz tills pitchdetektorn är färdig.

Delen ska senare ta emot **frekvens i Hz** och **tonHittad (true/false)** från pitchdetektorn. När ingen ton hittas döljs markören.

När detta fungerar bygger vi vidare med poäng och en tidslinje med låtens måltoner.

## Köra pitchtestet

Öppna `Singstar/Assets/Scenes/PitchTestScene.unity` i Unity.
Markera `PitchTest`, välj Test Frequency i Inspector och tryck Play.
Resultatet skrivs en gång i Console. Testet spelar inte upp ljud och behöver ingen mikrofon.
Med 200 Hz och 48000 Hz samplingsfrekvens ska resultatet bli 200 Hz.
Ändra frekvensen före Play; heltalslag kan ge en liten frekvensavvikelse.

Nuvarande API är `new PitchDetection().DetectPitch(samples, sampleRate)`:
det returnerar Hz, och 0 betyder ingen hittad ton. `SingstarController` använder
`frequency > 0f` som markering för hittad ton tills ett separat resultatformat införs.
`InputSampling.GetAudioSamples()` ger ett monoblock eller null innan data finns;
`GetSampleRate()` ger mikrofonklippets faktiska samplingsfrekvens.

Använd separata testscener när ni arbetar parallellt.
