import { test } from "@playwright/test";
import * as fs from "fs";

const SAMPLE_POINTS: [string, number, number][] = [
  ["cx640y460_below", 640, 460], // pavimento sotto player
  ["cx640y260_above", 640, 260], // pavimento sopra player
  ["cx530y360_left", 530, 360], // pavimento a sinistra player
  ["cx750y360_right", 750, 360], // pavimento a destra player
  ["cx200y150_dark", 200, 150], // angolo buio
  ["cx1100y580_dark", 1100, 580], // altro angolo buio
];

async function samplePixels(
  page: import("@playwright/test").Page,
): Promise<Record<string, string>> {
  return page.evaluate(async (pts: [string, number, number][]) => {
    const canvas = document.querySelector("canvas") as HTMLCanvasElement | null;
    if (!canvas) return { error: "no-canvas" };
    await new Promise((r) =>
      requestAnimationFrame(() => requestAnimationFrame(r)),
    );
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
    (globalThis as unknown as Record<string, unknown>)[
      "__MIDD_RENDER_DEBUG__"
    ] = {
      enabled: true,
      sampleLuma: false,
      logEveryNFrames: 60,
      disableMultiply: false,
      forceAmbient: undefined,
      singleLightIndex: undefined,
      hideLightmap: false,
      showGroundRT: false,
      disableSpecular: false,
    };
    const origMatchMedia = window.matchMedia.bind(window);
    window.matchMedia = (query: string) => {
      if (query === "(hover: hover) and (pointer: fine)") {
        return {
          matches: true,
          media: query,
          onchange: null,
          addListener: () => {},
          removeListener: () => {},
          addEventListener: () => {},
          removeEventListener: () => {},
          dispatchEvent: () => false,
        } as MediaQueryList;
      }
      return origMatchMedia(query);
    };
  });

  await page.goto("http://localhost:5173/mark-play/#/game/default/1");
  await page.waitForTimeout(5_000);

  // Log lightmap pixel direttamente
  const lightmapPixels = await page.evaluate(async () => {
    await new Promise((r) =>
      requestAnimationFrame(() => requestAnimationFrame(r)),
    );
    const canvas = document.querySelector("canvas") as HTMLCanvasElement | null;
    if (!canvas) return null;
    // Sample da 3 punti: player area, brazier area, dark area
    const tmp = document.createElement("canvas");
    tmp.width = canvas.width;
    tmp.height = canvas.height;
    const ctx = tmp.getContext("2d")!;
    ctx.drawImage(canvas, 0, 0);
    const pts: Record<string, string> = {};
    for (const [name, x, y] of [
      ["player_640_360", 640, 360] as [string, number, number],
      ["below_640_460", 640, 460] as [string, number, number],
      ["brazier_530_360", 530, 360] as [string, number, number],
    ]) {
      const d = ctx.getImageData(x, y, 1, 1).data;
      pts[name] = `(${d[0]},${d[1]},${d[2]})`;
    }
    return pts;
  });
  console.log("[canvas-pre-test] " + JSON.stringify(lightmapPixels));

  // Log dimensione canvas
  const canvasDims = await page.evaluate(() => {
    const canvas = document.querySelector("canvas") as HTMLCanvasElement | null;
    if (!canvas) return null;
    return {
      w: canvas.width,
      h: canvas.height,
      cssW: canvas.offsetWidth,
      cssH: canvas.offsetHeight,
      dpr: window.devicePixelRatio,
      scaleMode: canvas.dataset.scaleMode ?? null,
      rawScale: canvas.dataset.rawScale ?? null,
      appliedScale: canvas.dataset.appliedScale ?? null,
      renderWidth: canvas.dataset.renderWidth ?? null,
      renderHeight: canvas.dataset.renderHeight ?? null,
    };
  });
  console.log("[canvas-dims] " + JSON.stringify(canvasDims));

  // 1. Campionamento pipeline completa (NM + lightmap)
  const fullPipeline = await samplePixels(page);
  console.log("[pixels:full-pipeline] " + JSON.stringify(fullPipeline));

  // 2. Campionamento ground RT grezzo (mostra solo _groundRT senza NM)
  await page.evaluate(() => {
    type DbgConfig = { showGroundRT: boolean; hideLightmap: boolean };
    const dbg = (globalThis as unknown as Record<string, DbgConfig>)[
      "__MIDD_RENDER_DEBUG__"
    ];
    if (dbg) dbg.showGroundRT = true;
  });
  await page.waitForTimeout(200);
  const groundRT = await samplePixels(page);
  console.log("[pixels:ground-rt] " + JSON.stringify(groundRT));

  // 3. Campionamento NM ground senza lightmap
  await page.evaluate(() => {
    type DbgConfig = { showGroundRT: boolean; hideLightmap: boolean };
    const dbg = (globalThis as unknown as Record<string, DbgConfig>)[
      "__MIDD_RENDER_DEBUG__"
    ];
    if (dbg) {
      dbg.showGroundRT = false;
      dbg.hideLightmap = true;
    }
  });
  await page.waitForTimeout(200);
  const nmGround = await samplePixels(page);
  console.log("[pixels:nm-ground-no-lightmap] " + JSON.stringify(nmGround));

  // 4a. Campiona specular lightmask (nascondendo lightmap per vedere solo il RT)
  await page.evaluate(() => {
    type DbgConfig = {
      showSpecLightmask: boolean;
      showSpecMasked: boolean;
      hideLightmap: boolean;
      disableSpecular: boolean;
      singleLightIndex: number | undefined;
      forceAmbient: undefined;
    };
    const dbg = (globalThis as unknown as Record<string, DbgConfig>)[
      "__MIDD_RENDER_DEBUG__"
    ];
    if (dbg) {
      dbg.showSpecLightmask = true;
      dbg.showSpecMasked = false;
      dbg.hideLightmap = true;
      dbg.disableSpecular = false;
      dbg.singleLightIndex = undefined;
      dbg.forceAmbient = undefined;
    }
  });
  await page.waitForTimeout(200);
  const specLightmask = await samplePixels(page);
  console.log("[pixels:spec-lightmask] " + JSON.stringify(specLightmask));
  await page.screenshot({ path: "debug/screenshot-spec-lightmask.png" });

  // 4b. Campiona specular masked (spec × lightmask), nascondendo lightmap
  await page.evaluate(() => {
    type DbgConfig = {
      showSpecLightmask: boolean;
      showSpecMasked: boolean;
      hideLightmap: boolean;
    };
    const dbg = (globalThis as unknown as Record<string, DbgConfig>)[
      "__MIDD_RENDER_DEBUG__"
    ];
    if (dbg) {
      dbg.showSpecLightmask = false;
      dbg.showSpecMasked = true;
      dbg.hideLightmap = true;
    }
  });
  await page.waitForTimeout(200);
  const specMasked = await samplePixels(page);
  console.log("[pixels:spec-masked] " + JSON.stringify(specMasked));
  await page.screenshot({ path: "debug/screenshot-spec-masked.png" });

  // Reset
  await page.evaluate(() => {
    type DbgConfig = {
      showSpecLightmask: boolean;
      showSpecMasked: boolean;
      hideLightmap: boolean;
    };
    const dbg = (globalThis as unknown as Record<string, DbgConfig>)[
      "__MIDD_RENDER_DEBUG__"
    ];
    if (dbg) {
      dbg.showSpecLightmask = false;
      dbg.showSpecMasked = false;
      dbg.hideLightmap = false;
    }
  });

  // 4c. Test ambient-only: forceAmbient=white, disableSpecular, hideLightmap → output = ground-raw * 1 = ground-raw
  await page.evaluate(() => {
    type DbgConfig = {
      showGroundRT: boolean;
      hideLightmap: boolean;
      disableSpecular: boolean;
      singleLightIndex: number | undefined;
      forceAmbient: { r: number; g: number; b: number } | undefined;
    };
    const dbg = (globalThis as unknown as Record<string, DbgConfig>)[
      "__MIDD_RENDER_DEBUG__"
    ];
    if (dbg) {
      dbg.showGroundRT = false;
      dbg.hideLightmap = true;
      dbg.disableSpecular = true;
      dbg.singleLightIndex = 0;
      dbg.forceAmbient = { r: 1, g: 1, b: 1 };
    }
  });
  await page.waitForTimeout(200);
  const ambientWhite = await samplePixels(page);
  console.log(
    "[pixels:ambient-white-single-light] " + JSON.stringify(ambientWhite),
  );

  // Reset forceAmbient
  await page.evaluate(() => {
    type DbgConfig = { forceAmbient: undefined };
    const dbg = (globalThis as unknown as Record<string, DbgConfig>)[
      "__MIDD_RENDER_DEBUG__"
    ];
    if (dbg) dbg.forceAmbient = undefined;
  });

  // Screenshot lightmap grezzo
  await page.evaluate(() => {
    type DbgConfig = {
      showLightmap: boolean;
      showGroundRT: boolean;
      hideLightmap: boolean;
      disableSpecular: boolean;
      singleLightIndex: number | undefined;
    };
    const dbg = (globalThis as unknown as Record<string, DbgConfig>)[
      "__MIDD_RENDER_DEBUG__"
    ];
    if (dbg) {
      dbg.showLightmap = true;
      dbg.showGroundRT = false;
      dbg.hideLightmap = false;
      dbg.disableSpecular = false;
      dbg.singleLightIndex = undefined;
    }
  });
  await page.waitForTimeout(200);
  const lightmapRaw = await samplePixels(page);
  console.log("[pixels:lightmap-raw] " + JSON.stringify(lightmapRaw));
  await page.screenshot({ path: "debug/screenshot-lightmap.png" });
  await page.evaluate(() => {
    type DbgConfig = { showLightmap: boolean };
    const dbg = (globalThis as unknown as Record<string, DbgConfig>)[
      "__MIDD_RENDER_DEBUG__"
    ];
    if (dbg) dbg.showLightmap = false;
  });

  // 4. Solo luce player (singleLightIndex=0) + no-specular, no-lightmap per isolamento
  await page.evaluate(() => {
    type DbgConfig = {
      showGroundRT: boolean;
      hideLightmap: boolean;
      disableSpecular: boolean;
      singleLightIndex: number | undefined;
    };
    const dbg = (globalThis as unknown as Record<string, DbgConfig>)[
      "__MIDD_RENDER_DEBUG__"
    ];
    if (dbg) {
      dbg.showGroundRT = false;
      dbg.hideLightmap = true;
      dbg.disableSpecular = true;
      dbg.singleLightIndex = 0;
    }
  });
  await page.waitForTimeout(200);
  const playerLightOnly = await samplePixels(page);
  console.log(
    "[pixels:player-light-only-no-spec-no-lm] " +
      JSON.stringify(playerLightOnly),
  );

  // 5. Campionamento senza specular (full lightmap)
  await page.evaluate(() => {
    type DbgConfig = {
      showGroundRT: boolean;
      hideLightmap: boolean;
      disableSpecular: boolean;
      singleLightIndex: number | undefined;
    };
    const dbg = (globalThis as unknown as Record<string, DbgConfig>)[
      "__MIDD_RENDER_DEBUG__"
    ];
    if (dbg) {
      dbg.showGroundRT = false;
      dbg.hideLightmap = false;
      dbg.disableSpecular = true;
      dbg.singleLightIndex = undefined;
    }
  });
  await page.waitForTimeout(200);
  const noSpecular = await samplePixels(page);
  console.log("[pixels:no-specular] " + JSON.stringify(noSpecular));
  await page.screenshot({ path: "debug/screenshot-no-specular.png" });

  // Screenshot pipeline completa
  await page.evaluate(() => {
    type DbgConfig = {
      showGroundRT: boolean;
      hideLightmap: boolean;
      disableSpecular: boolean;
    };
    const dbg = (globalThis as unknown as Record<string, DbgConfig>)[
      "__MIDD_RENDER_DEBUG__"
    ];
    if (dbg) {
      dbg.showGroundRT = false;
      dbg.hideLightmap = false;
      dbg.disableSpecular = false;
    }
  });
  await page.waitForTimeout(200);
  await page.screenshot({ path: "debug/screenshot.png" });

  // Stampa un riepilogo comparativo
  console.log(
    "[comparison] pos | ground-raw | nm-ground | full-pipeline | no-specular",
  );
  for (const [name] of SAMPLE_POINTS) {
    console.log(
      `[comparison] ${name} | ${groundRT[name]} | ${nmGround[name]} | ${fullPipeline[name]} | ${noSpecular[name]}`,
    );
  }

  fs.writeFileSync("debug/game-debug-output.txt", logs.join("\n"));
});
