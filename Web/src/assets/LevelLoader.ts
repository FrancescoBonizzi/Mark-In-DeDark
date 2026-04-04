import { AssetsRoot } from './constants';

export const PREVIEW_FOLDER = 'preview';
export const PREVIEW_SESSION_KEY = 'editor-preview';

export const loadLevelText = async (folder: string, levelId: number): Promise<string> => {
    // Preview dal Level Editor: carica da sessionStorage
    if (folder === PREVIEW_FOLDER) {
        const saved = sessionStorage.getItem(PREVIEW_SESSION_KEY);
        if (!saved) throw new Error('Nessun livello di preview in sessionStorage. Usa il pulsante "Testa in gioco" nel Level Editor.');
        return saved;
    }
    const url = `${AssetsRoot}assets/levels/${folder}/${levelId}.mark`;
    const response = await fetch(url);
    if (!response.ok) throw new Error(`Failed to load level ${folder}/${levelId}`);
    return response.text();
};
