import { test, expect } from "@playwright/test";
import * as fs from "fs";

test("Cattura output console del Level Editor", async ({ page }) => {
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

  // Override matchMedia per mouse desktop (non touch)
  await page.addInitScript(() => {
    const origMatchMedia = window.matchMedia.bind(window);
    window.matchMedia = (query: string) => {
      if (query === "(pointer: coarse)") {
        return {
          matches: false,
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

  await page.goto("http://localhost:5173/mark-in-dedark/#/level-editor");

  // 1. Verifica griglia
  await page.waitForSelector(".editor-grid", { timeout: 10_000 });
  logs.push("[test] Griglia .editor-grid trovata");

  const cellCount = await page.locator(".editor-cell").count();
  logs.push(`[test] Celle trovate: ${cellCount} (attese: 1125)`);
  expect(cellCount).toBe(1125);

  // 2. Verifica sidebar e toolbar
  expect(await page.locator(".editor-sidebar").isVisible()).toBe(true);
  expect(await page.locator(".editor-toolbar").isVisible()).toBe(true);
  logs.push("[test] Sidebar e toolbar visibili");

  // 3. Verifica che Terra1 sia selezionata di default
  const terra1Radio = await page.locator('input[name="editor-tool-ground"][value="1"]');
  expect(await terra1Radio.isChecked()).toBe(true);
  logs.push("[test] Terra1 selezionata di default ✓");

  // 4. Click su una cella: la cella centrale (col=22, row=12)
  const targetCell = await page.locator('[data-col="22"][data-row="12"]');
  await targetCell.click();
  await page.waitForTimeout(100);

  // Verifica che il colore sia cambiato a Terra1 (#6E2233)
  const bgColor = await targetCell.evaluate(el => {
    return window.getComputedStyle(el).backgroundColor;
  });
  logs.push(`[test] Colore cella dopo click Terra1: ${bgColor}`);
  // rgb(110, 34, 51) = #6E2233
  expect(bgColor).toBe("rgb(110, 34, 51)");

  // 5. Drag painting: click su altra cella e muovi verso destra
  const startCell = await page.locator('[data-col="5"][data-row="5"]');
  const endCell = await page.locator('[data-col="8"][data-row="5"]');
  await startCell.hover();
  await page.mouse.down();
  await page.locator('[data-col="6"][data-row="5"]').hover();
  await page.locator('[data-col="7"][data-row="5"]').hover();
  await endCell.hover();
  await page.mouse.up();
  await page.waitForTimeout(100);

  const draggedCellColor = await page.locator('[data-col="6"][data-row="5"]').evaluate(el => {
    return window.getComputedStyle(el).backgroundColor;
  });
  logs.push(`[test] Colore cella dopo drag: ${draggedCellColor}`);
  expect(draggedCellColor).toBe("rgb(110, 34, 51)");

  // 6. Cambia a tab Muri e verifica
  await page.locator('.editor-tab-btn:nth-child(2)').click();
  await page.waitForTimeout(100);
  const wallPanelVisible = await page.locator('[data-panel="wall"]').isVisible();
  expect(wallPanelVisible).toBe(true);
  logs.push("[test] Tab Muri switchato ✓");

  // 7. Screenshot finale
  await page.waitForTimeout(2_000);
  await page.screenshot({ path: "debug/leveleditor-screenshot.png" });
  logs.push("[test] Screenshot salvato");

  fs.writeFileSync("debug/leveleditor-debug-output.txt", logs.join("\n"));
});
