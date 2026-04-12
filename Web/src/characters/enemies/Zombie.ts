import { Container, Spritesheet } from "pixi.js";
import { PatrolAI } from "../../ai/PatrolAI";
import { GameCell } from "../../grid/GameCell";
import { GameGrid } from "../../grid/GameGrid";
import { MarkLight } from "../../lighting/MarkLight";
import { CandlesManager } from "../../objects/CandlesManager";
import { Enemy, EnemyConfig } from "../Enemy";
import { Player } from "../Player";
import { buildEnemyAnimations } from "./enemyAnimations";

const SPEED_S = 0.8;
const TORCH_OFFSETS = {
  up: { x: 0, y: -10 },
  left: { x: 0, y: -20 },
  right: { x: 0, y: -10 },
  down: { x: 0, y: -10 },
};

export class Zombie extends Enemy {
  constructor(
    grid: GameGrid,
    sheets: Spritesheet[],
    objectsContainer: Container,
    objectsNormalContainer: Container,
    startX: number,
    startY: number,
    waypoints: GameCell[],
    player: Player,
    candlesManager: CandlesManager,
  ) {
    const animations = buildEnemyAnimations(
      sheets,
      "Zombie/zombie",
      { front: 4, back: 4, side: 4 },
      { front: 4, back: 4, side: 4 },
    );
    animations.play("IdleDown");

    const torch = new MarkLight({
      x: startX + TORCH_OFFSETS.down.x,
      y: startY + TORCH_OFFSETS.down.y,
      z: 31, // C# 46 × (2/3) = 30.7 ≈ 31
      opacity: 0.5,
      radius: 300,
      castsShadows: false,
      color1: 0x8bef5a,
      color2: 0x6dbc45,
      distanceSquared: 2_200, // C# 4_950 × (2/3)²
    });

    const ai = new PatrolAI(waypoints, grid, candlesManager, player, true);

    const config: EnemyConfig = {
      startX,
      startY,
      movementSpeed: SPEED_S,
      animations,
      canGoOverWalls: false,
      torch: { light: torch, offsets: TORCH_OFFSETS },
      initialAI: ai,
      killsCandlesOnCollision: true,
      nCellsCandleRange: 2,
      player,
      candlesManager,
    };
    super(grid, objectsContainer, objectsNormalContainer, config);
  }
}
