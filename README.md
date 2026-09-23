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
