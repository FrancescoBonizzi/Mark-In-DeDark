import { test } from "@playwright/test";
import * as fs from "fs";

test("Cattura output console del gioco", async ({ page }) => {
  const logs: string[] = [];

  // Ascolta i messaggi della console (log, error, warn)
  page.on("console", (msg) => {
    const text = `[${msg.type()}] ${msg.text()}`;
    logs.push(text);
    console.debug(text); // Lo vedi anche nel tuo terminale
  });

  // Vai alla pagina del gioco
  await page.goto("http://localhost:5173/mark-in-dedark/#/game/3");

  // Aspetta un tempo stabilito per far girare il gioco (es. 5 secondi)
  // O puoi aspettare che accada qualcosa di specifico nel gioco
  await page.waitForTimeout(10_000);

  // Salva i log in un file di testo
  fs.writeFileSync("debug/game-debug-output.txt", logs.join("\n"));
});
