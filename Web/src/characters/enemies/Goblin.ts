import { Container, Spritesheet } from "pixi.js";
import { PlayerHunterAI } from "../../ai/PlayerHunterAI";
import { GameGrid } from "../../grid/GameGrid";
import { MarkLight } from "../../lighting/MarkLight";
import { CandlesManager } from "../../objects/CandlesManager";
import { Enemy, EnemyConfig } from "../Enemy";
import { Player } from "../Player";
import { buildEnemyAnimations } from "./enemyAnimations";

const BASE_SPEED_S = 0.8;

const TORCH_OFFSETS = {
  up: { x: -35, y: -10 },
  left: { x: 5, y: 5 },
  right: { x: -5, y: 5 },
  down: { x: 30, y: -15 },
};

export class Goblin extends Enemy {
  constructor(
    grid: GameGrid,
    sheets: Spritesheet[],
    objectsContainer: Container,
    objectsNormalContainer: Container,
    startX: number,
    startY: number,
    player: Player,
    candlesManager: CandlesManager,
  ) {
    const animations = buildEnemyAnimations(
      sheets,
      "Goblin/goblin",
      { front: 4, back: 4, side: 4 },
      { front: 5, back: 5, side: 5 },
    );
    animations.play("IdleDown");

    const torch = new MarkLight({
      x: startX + TORCH_OFFSETS.down.x,
      y: startY + TORCH_OFFSETS.down.y,
      z: 31, // C# 46 × (2/3) = 30.7 ≈ 31
      opacity: 0.5,
      radius: 300,
      castsShadows: false,
      color1: 0xcaf076,
      color2: 0xc3e27c,
      distanceSquared: 2_200, // C# 4_950 × (2/3)²
    });

    const ai = new PlayerHunterAI(player, grid, candlesManager);

    const config: EnemyConfig = {
      startX,
      startY,
      movementSpeed: BASE_SPEED_S,
      animations,
      canGoOverWalls: false,
      torch: { light: torch, offsets: TORCH_OFFSETS },
      initialAI: ai,
      killsCandlesOnCollision: true,
      nCellsCandleRange: 3,
      player,
      candlesManager,
    };
    super(grid, objectsContainer, objectsNormalContainer, config);
  }
}
