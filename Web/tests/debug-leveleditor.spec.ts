import { test, expect } from "@playwright/test";
import * as fs from "fs";

// ---------------------------------------------------------------------------
// Helper: pagina con matchMedia override per desktop
// ---------------------------------------------------------------------------

async function setupPage(page: import("@playwright/test").Page) {
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
}

/** Clicca la label associata a un radio nascosto (display:none). */
async function clickToolLabel(page: import("@playwright/test").Page, radioId: string) {
  await page.locator(`label[for="${radioId}"]`).click();
}

// ---------------------------------------------------------------------------
// Test 1: Griglia, colori, drag painting, tab Muri
// ---------------------------------------------------------------------------

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

  await setupPage(page);
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

// ---------------------------------------------------------------------------
// Test 2: Posizionamento oggetti (Giocatore singleton + cancella)
// ---------------------------------------------------------------------------

test("Posizionamento oggetti nel Level Editor", async ({ page }) => {
  const logs: string[] = [];
  page.on("console", (msg) => logs.push(`[${msg.type()}] ${msg.text()}`));
  page.on("pageerror", (err) => logs.push(`[pageerror] ${err.message}`));

  await setupPage(page);
  await page.goto("http://localhost:5173/mark-in-dedark/#/level-editor");
  await page.waitForSelector(".editor-grid", { timeout: 10_000 });

  // --- Switch al tab Oggetti ---
  await page.locator('.editor-tab-btn:nth-child(3)').click();
  await page.waitForTimeout(100);
  expect(await page.locator('[data-panel="obj"]').isVisible()).toBe(true);
  logs.push("[test] Tab Oggetti visibile ✓");

  // --- Seleziona Giocatore cliccando la label (radio è display:none) ---
  await clickToolLabel(page, 'obj-radio-1');
  await page.waitForTimeout(50);
  const playerRadio = page.locator('#obj-radio-1');
  expect(await playerRadio.isChecked()).toBe(true);
  logs.push("[test] Radio Giocatore selezionato ✓");

  // --- Piazza il Giocatore su una cella (auto-set Terra1 se NonCella) ---
  const targetCell = page.locator('[data-col="10"][data-row="10"]');
  await targetCell.click();
  await page.waitForTimeout(100);

  // Verifica che l'emoji del Giocatore sia nella cella
  const cellContent = await targetCell.locator('.cell-object').textContent();
  logs.push(`[test] Emoji cella: "${cellContent}"`);
  expect(cellContent).toBe('🧍');
  logs.push("[test] Giocatore piazzato ✓");

  // --- Verifica che il radio Giocatore sia ora disabilitato (singleton) ---
  const isDisabled = await playerRadio.isDisabled();
  expect(isDisabled).toBe(true);
  logs.push("[test] Radio Giocatore disabilitato (singleton) ✓");

  // --- Seleziona Cancella cliccando la label ---
  await clickToolLabel(page, 'obj-radio-0');
  await page.waitForTimeout(50);
  await targetCell.click();
  await page.waitForTimeout(100);

  // Verifica che l'emoji sia sparita
  const cellContentAfterDelete = await targetCell.locator('.cell-object').count();
  expect(cellContentAfterDelete).toBe(0);
  logs.push("[test] Cancella oggetto funziona ✓");

  // --- Ora il Giocatore deve essere di nuovo abilitabile ---
  const isDisabledAfterDelete = await playerRadio.isDisabled();
  expect(isDisabledAfterDelete).toBe(false);
  logs.push("[test] Radio Giocatore riabilitato dopo cancellazione ✓");

  fs.writeFileSync("debug/leveleditor-objects-output.txt", logs.join("\n"));
});

// ---------------------------------------------------------------------------
// Test 3: Shortcut tastiera → switch tab
// ---------------------------------------------------------------------------

test("Shortcut tastiera cambiano tab attivo nel Level Editor", async ({ page }) => {
  const logs: string[] = [];

  await setupPage(page);
  await page.goto("http://localhost:5173/mark-in-dedark/#/level-editor");
  await page.waitForSelector(".editor-grid", { timeout: 10_000 });

  // Clicca sulla griglia per togliere focus dagli input della toolbar
  await page.locator('[data-col="2"][data-row="2"]').click();
  await page.waitForTimeout(100);

  // F → tab Terra
  await page.keyboard.press('f');
  await page.waitForTimeout(100);
  expect(await page.locator('[data-panel="ground"]').isVisible()).toBe(true);
  logs.push("[test] F → tab Terra ✓");

  // W → tab Muri
  await page.keyboard.press('w');
  await page.waitForTimeout(100);
  expect(await page.locator('[data-panel="wall"]').isVisible()).toBe(true);
  logs.push("[test] W → tab Muri ✓");

  // D → tab Muri (D seleziona NoMuro)
  await page.keyboard.press('d');
  await page.waitForTimeout(100);
  expect(await page.locator('[data-panel="wall"]').isVisible()).toBe(true);
  logs.push("[test] D → tab Muri (NoMuro) ✓");

  // C → tab Oggetti
  await page.keyboard.press('c');
  await page.waitForTimeout(100);
  expect(await page.locator('[data-panel="obj"]').isVisible()).toBe(true);
  logs.push("[test] C → tab Oggetti ✓");

  // R → tab Terra (Riempimento1)
  await page.keyboard.press('r');
  await page.waitForTimeout(100);
  expect(await page.locator('[data-panel="ground"]').isVisible()).toBe(true);
  logs.push("[test] R → tab Terra (Riempimento1) ✓");

  fs.writeFileSync("debug/leveleditor-shortcuts-output.txt", logs.join("\n"));
});

// ---------------------------------------------------------------------------
// Test 4: Testo (stringCode) → piazza marker "A" in cella
// ---------------------------------------------------------------------------

test("Inserimento testo in cella nel Level Editor", async ({ page }) => {
  const logs: string[] = [];
  page.on("console", (msg) => logs.push(`[${msg.type()}] ${msg.text()}`));
  page.on("pageerror", (err) => logs.push(`[pageerror] ${err.message}`));

  await setupPage(page);

  // Intercetta il prompt PRIMA della navigazione
  page.on('dialog', async dialog => {
    logs.push(`[test] Dialog: tipo=${dialog.type()} default="${dialog.defaultValue()}"`);
    await dialog.accept('Testo di test');
  });

  await page.goto("http://localhost:5173/mark-in-dedark/#/level-editor");
  await page.waitForSelector(".editor-grid", { timeout: 10_000 });

  // Vai a tab Oggetti e seleziona Testo (value=-1)
  await page.locator('.editor-tab-btn:nth-child(3)').click();
  await page.waitForTimeout(100);
  await clickToolLabel(page, 'obj-radio--1');
  await page.waitForTimeout(50);
  expect(await page.locator('#obj-radio--1').isChecked()).toBe(true);
  logs.push("[test] Radio Testo selezionato ✓");

  // Clicca su una cella (auto-set Terra1 se NonCella grazie a setStringCode)
  const targetCell = page.locator('[data-col="15"][data-row="10"]');
  await targetCell.click();
  await page.waitForTimeout(300);

  // Verifica che il marker "A" sia nella cella
  const markerCount = await targetCell.locator('.cell-text-marker').count();
  logs.push(`[test] Marker "A" trovati: ${markerCount}`);
  expect(markerCount).toBe(1);
  logs.push("[test] Testo piazzato correttamente ✓");

  fs.writeFileSync("debug/leveleditor-text-output.txt", logs.join("\n"));
});

// ---------------------------------------------------------------------------
// Test 5: Waypoint nemici auto-generati
// ---------------------------------------------------------------------------

test("Waypoint nemici auto-generati nel Level Editor", async ({ page }) => {
  const logs: string[] = [];
  page.on("console", (msg) => logs.push(`[${msg.type()}] ${msg.text()}`));
  page.on("pageerror", (err) => logs.push(`[pageerror] ${err.message}`));

  // Intercetta tutti i prompt PRIMA della navigazione
  const promptDefaults: string[] = [];
  page.on('dialog', async dialog => {
    const def = dialog.defaultValue();
    promptDefaults.push(def);
    logs.push(`[test] Prompt waypoint: default="${def}"`);
    await dialog.accept(def); // accetta il valore auto-generato
  });

  await setupPage(page);
  await page.goto("http://localhost:5173/mark-in-dedark/#/level-editor");
  await page.waitForSelector(".editor-grid", { timeout: 10_000 });

  // Dipingi Terra1 su 4 celle (per avere terreno su cui piazzare nemici)
  for (const col of [5, 6, 7, 8]) {
    await page.locator(`[data-col="${col}"][data-row="15"]`).click();
    await page.waitForTimeout(30);
  }

  // Vai al tab Nemici
  await page.locator('.editor-tab-btn:nth-child(4)').click();
  await page.waitForTimeout(100);
  expect(await page.locator('[data-panel="enemy"]').isVisible()).toBe(true);
  logs.push("[test] Tab Nemici visibile ✓");

  // Seleziona Zombie cliccando la label (radio è display:none)
  await clickToolLabel(page, 'obj-radio-5');
  await page.waitForTimeout(50);
  expect(await page.locator('#obj-radio-5').isChecked()).toBe(true);
  logs.push("[test] Zombie selezionato ✓");

  // Piazza primo zombie
  await page.locator('[data-col="5"][data-row="15"]').click();
  await page.waitForTimeout(200);

  // Piazza secondo zombie
  await page.locator('[data-col="6"][data-row="15"]').click();
  await page.waitForTimeout(200);

  // Verifica che i default siano stati Z-1-1 e Z-1-2
  logs.push(`[test] Prompt defaults ricevuti: ${promptDefaults.join(', ')}`);
  expect(promptDefaults[0]).toBe('Z-1-1');
  expect(promptDefaults[1]).toBe('Z-1-2');
  logs.push("[test] Auto-generazione waypoint: Z-1-1, Z-1-2 ✓");

  // Verifica che l'emoji zombie sia visibile nelle celle
  const emoji1 = await page.locator('[data-col="5"][data-row="15"] .cell-object').textContent();
  const emoji2 = await page.locator('[data-col="6"][data-row="15"] .cell-object').textContent();
  expect(emoji1).toBe('🧟');
  expect(emoji2).toBe('🧟');
  logs.push("[test] Emoji zombie visibili ✓");

  fs.writeFileSync("debug/leveleditor-waypoints-output.txt", logs.join("\n"));
});
