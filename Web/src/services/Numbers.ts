import Interval from "../primitives/Interval.ts";
import {Point} from "pixi.js";

const randomBetween = (min: number, max: number) => {

    if (min === max)
        return min;

    if (min > max)
        [min, max] = [max, min];

    return min + Math.random() * (max - min);
};

export default {

    randomBetween: randomBetween,

    randomBetweenInterval: (interval: Interval) => {

        if (interval.min === interval.max)
            return interval.min;

        if (interval.min > interval.max)
            [interval.min, interval.max] = [interval.max, interval.min];
        return randomBetween(interval.min, interval.max);
    },

    headOrTail: () => {
        return Math.random() > 0.5;
    },

    toRadians: (degrees: number) => {
        return degrees * Math.PI / 180;
    },

    lerp: (start: number, end: number, t: number): number => {
        return start + t * (end - start);
    },

    clamp01: (n: number) => Math.max(0, Math.min(1, n)),

    clamp: (n: number, min: number, max: number) => Math.max(min, Math.min(max, n)),

    easeOutCubic: (t: number) => {
        return 1 - Math.pow(1 - t, 3);
    },

    easeInCubic: (t: number) => {
        return t * t * t;
    },

    addPoints(a: Point, b: Point): Point {
        return new Point(a.x + b.x, a.y + b.y);
    }
}
