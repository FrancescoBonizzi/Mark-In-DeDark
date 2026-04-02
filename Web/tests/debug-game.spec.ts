import { test } from "@playwright/test";
import * as fs from "fs";

test("Cattura output console del gioco", async ({ page }) => {
  const logs: string[] = [];

  // Ascolta i messaggi della console (log, error, warn)
  page.on("console", (msg) => {
    const text = `[${msg.type()}] ${msg.text()}`;
    logs.push(text);
    console.debug(text);
  });

  page.on("pageerror", (err) => {
    const text = `[pageerror] ${err.message}\n${err.stack}`;
    logs.push(text);
    console.debug(text);
  });

  // Attiva il debug rendering e simula una tastiera fisica prima di caricare il gioco
  await page.addInitScript(() => {
    (globalThis as unknown as Record<string, unknown>)["__MIDD_RENDER_DEBUG__"] = {
      enabled: true,
      sampleLuma: false,
      logEveryNFrames: 60,
      disableMultiply: true,
      forceAmbient: { r: 0, g: 0, b: 0 },
      singleLightIndex: 16,
    };
    // Forza hasPhysicalKeyboard() a ritornare true (Playwright non ha puntatore fisico)
    const origMatchMedia = window.matchMedia.bind(window);
    window.matchMedia = (query: string) => {
      if (query === "(hover: hover) and (pointer: fine)") {
        return { matches: true, media: query, onchange: null,
          addListener: () => {}, removeListener: () => {},
          addEventListener: () => {}, removeEventListener: () => {},
          dispatchEvent: () => false,
        } as MediaQueryList;
      }
      return origMatchMedia(query);
    };
  });

  // Vai alla pagina del gioco
  await page.goto("http://localhost:5173/mark-in-dedark/#/game/3");

  // Aspetta un tempo stabilito per far girare il gioco
  await page.waitForTimeout(5_000);

  // Screenshot per analisi visiva
  await page.screenshot({ path: "debug/screenshot.png" });

  // Salva i log in un file di testo
  fs.writeFileSync("debug/game-debug-output.txt", logs.join("\n"));
});
