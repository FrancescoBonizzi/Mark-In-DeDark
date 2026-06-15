import {Howl, Howler} from 'howler';
import {AssetsRoot} from "../assets/constants.ts";

class SoundManager {
    private sounds: Record<string, Howl> = {};

    constructor() {
        Howler.autoUnlock = true;
    }

    playMenuMusic() {
        const key = 'musicMenu';
        if (!this.sounds[key]) {
            this.sounds[key] = new Howl({
                src: [`${AssetsRoot}sounds/music/menu.mp3`],
                loop: true,
                volume: 0.4,
                onloaderror: () => { delete this.sounds[key]; },
            });
        }

        if (this.sounds[key]?.playing())
            return;

        this.stopAllMusic();
        this.sounds[key]?.play();
    }

    playGameMusic() {
        const key = 'musicGame';
        if (!this.sounds[key]) {
            this.sounds[key] = new Howl({
                src: [`${AssetsRoot}sounds/music/game.mp3`],
                loop: true,
                volume: 0.4,
                onloaderror: () => { delete this.sounds[key]; },
            });
        }

        this.stopAllMusic();
        this.sounds[key]?.play();
    }

    stopAllMusic() {
        this.sounds['musicMenu']?.stop();
        this.sounds['musicGame']?.stop();
    }

    play(name: string, src: string, options?: { loop?: boolean; volume?: number }): void {
        if (!this.sounds[name]) {
            this.sounds[name] = new Howl({
                src: [src],
                loop: options?.loop ?? false,
                volume: options?.volume ?? 1,
                onloaderror: () => { delete this.sounds[name]; },
            });
        }
        if (this.sounds[name]?.playing()) return;
        this.sounds[name]?.play();
    }

    stop(name: string): void {
        this.sounds[name]?.stop();
    }

    stopAll(): void {
        for (const key in this.sounds) {
            this.sounds[key]?.stop();
        }
    }

    pauseAll(): void {
        for (const key in this.sounds) {
            this.sounds[key]?.pause();
        }
    }

    resumeAll(): void {
        for (const key in this.sounds) {
            const sound = this.sounds[key];
            if (sound && (sound as any)._sounds?.some((s: any) => s._paused)) {
                sound.play();
            }
        }
    }
}

export default SoundManager;
