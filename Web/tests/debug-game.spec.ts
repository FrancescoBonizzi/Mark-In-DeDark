import { test } from "@playwright/test";
import * as fs from "fs";

const SAMPLE_POINTS: [string, number, number][] = [
  ["cx640y460_below", 640, 460],   // pavimento sotto player
  ["cx640y260_above", 640, 260],   // pavimento sopra player
  ["cx530y360_left",  530, 360],   // pavimento a sinistra player
  ["cx750y360_right", 750, 360],   // pavimento a destra player
  ["cx200y150_dark",  200, 150],   // angolo buio
  ["cx1100y580_dark", 1100, 580],  // altro angolo buio
];

async function samplePixels(page: import("@playwright/test").Page): Promise<Record<string, string>> {
  return page.evaluate(async (pts: [string, number, number][]) => {
    const canvas = document.querySelector("canvas") as HTMLCanvasElement | null;
    if (!canvas) return { error: "no-canvas" };
    await new Promise(r => requestAnimationFrame(() => requestAnimationFrame(r)));
    const tmp = document.createElement("canvas");
    tmp.width = canvas.width;
    tmp.height = canvas.height;
    const ctx = tmp.getContext("2d")!;
    ctx.drawImage(canvas, 0, 0);
    const result: Record<string, string> = {};
    for (const [name, x, y] of pts) {
      const d = ctx.getImageData(x, y, 1, 1).data;
      result[name] = `(${d[0]},${d[1]},${d[2]})`;
    }
    return result;
  }, SAMPLE_POINTS);
}

test("Cattura output console del gioco", async ({ page }) => {
  const logs: string[] = [];

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

  await page.goto("http://localhost:5173/mark-in-dedark/#/game/4");
  await page.waitForTimeout(5_000);

  // 1. Campionamento pipeline completa (NM + lightmap)
  const fullPipeline = await samplePixels(page);
  console.log("[pixels:full-pipeline] " + JSON.stringify(fullPipeline));

  // 2. Campionamento ground RT grezzo (mostra solo _groundRT senza NM)
  await page.evaluate(() => {
    type DbgConfig = { showGroundRT: boolean; hideLightmap: boolean };
    const dbg = (globalThis as unknown as Record<string, DbgConfig>)["__MIDD_RENDER_DEBUG__"];
    if (dbg) dbg.showGroundRT = true;
  });
  await page.waitForTimeout(200);
  const groundRT = await samplePixels(page);
  console.log("[pixels:ground-rt] " + JSON.stringify(groundRT));

  // 3. Campionamento NM ground senza lightmap
  await page.evaluate(() => {
    type DbgConfig = { showGroundRT: boolean; hideLightmap: boolean };
    const dbg = (globalThis as unknown as Record<string, DbgConfig>)["__MIDD_RENDER_DEBUG__"];
    if (dbg) { dbg.showGroundRT = false; dbg.hideLightmap = true; }
  });
  await page.waitForTimeout(200);
  const nmGround = await samplePixels(page);
  console.log("[pixels:nm-ground-no-lightmap] " + JSON.stringify(nmGround));

  // Screenshot per analisi visiva (ripristina pipeline normale)
  await page.evaluate(() => {
    type DbgConfig = { showGroundRT: boolean; hideLightmap: boolean };
    const dbg = (globalThis as unknown as Record<string, DbgConfig>)["__MIDD_RENDER_DEBUG__"];
    if (dbg) { dbg.showGroundRT = false; dbg.hideLightmap = false; }
  });
  await page.waitForTimeout(200);
  await page.screenshot({ path: "debug/screenshot.png" });

  // Stampa un riepilogo comparativo
  console.log("[comparison] pos | ground-raw | nm-ground | full-pipeline");
  for (const [name] of SAMPLE_POINTS) {
    console.log(`[comparison] ${name} | ${groundRT[name]} | ${nmGround[name]} | ${fullPipeline[name]}`);
  }

  fs.writeFileSync("debug/game-debug-output.txt", logs.join("\n"));
});
