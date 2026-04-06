import { test } from "@playwright/test";
import * as fs from "fs";

test("Ghost AI debug", async ({ page }) => {
  const logs: string[] = [];
  page.on("console", (msg) => { logs.push(msg.text()); });
  page.on("pageerror", (err) => { logs.push("[ERR] " + err.message); });

  await page.goto("http://localhost:5173/mark-in-dedark/#/game/debug/1");
  // Wait even if the page navigates (game over)
  await page.waitForTimeout(12_000).catch(() => {});

  fs.writeFileSync("debug/ghost-debug.txt", logs.join("\n"));
});
