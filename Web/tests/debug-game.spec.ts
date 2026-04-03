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
      disableMultiply: false,
      forceAmbient: undefined,
      singleLightIndex: undefined,
      hideLightmap: false,
      showGroundRT: false,
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
  await page.goto("http://localhost:5173/mark-in-dedark/#/game/1");

  // Aspetta un tempo stabilito per far girare il gioco
  await page.waitForTimeout(5_000);

  // Screenshot per analisi visiva
  await page.screenshot({ path: "debug/screenshot.png" });

  // Analisi pixel PNG: leggi il file e campiona pixel specifici
  // PNG è RGBA: offset = (y * width + x) * 4 dopo skip header
  // Usiamo un approccio base: cerca le sequenze RGB nel buffer PNG
  // Approach più semplice: usa canvas 2D in browser per leggere
  const canvasPixels = await page.evaluate(async () => {
    const canvas = document.querySelector("canvas") as HTMLCanvasElement | null;
    if (!canvas) return "no-canvas";
    // Aspetta il prossimo frame per avere i pixel freschi
    await new Promise(r => requestAnimationFrame(() => requestAnimationFrame(r)));
    const tmp = document.createElement("canvas");
    tmp.width = canvas.width;
    tmp.height = canvas.height;
    const ctx = tmp.getContext("2d")!;
    ctx.drawImage(canvas, 0, 0);
    const result: Record<string, string> = {};
    for (const [name, x, y] of [["cx640y300", 640, 300], ["cx640y360", 640, 360], ["cx640y400", 640, 400], ["cx320y240", 320, 240], ["cx400y300", 400, 300], ["cx500y360", 500, 360]] as [string, number, number][]) {
      const d = ctx.getImageData(x, y, 1, 1).data;
      result[name] = "(" + d[0] + "," + d[1] + "," + d[2] + ")";
    }
    return JSON.stringify(result);
  });
  console.log("[canvas-pixels] " + canvasPixels);

  // Salva i log in un file di testo
  fs.writeFileSync("debug/game-debug-output.txt", logs.join("\n"));
});
