import { Container, Texture } from "pixi.js";
import { GameCell } from "../grid/GameCell";
import { PassableObject } from "./PassableObject";

export class UnpassableObject extends PassableObject {
  constructor(texture: Texture, gameCell: GameCell, container: Container) {
    gameCell.isUnwalkable = true;
    super(texture, gameCell, container);
  }
}
