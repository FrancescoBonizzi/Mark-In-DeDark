import { AssetsRoot } from './constants';

export const PREVIEW_FOLDER = 'preview';
export const PREVIEW_LOCAL_KEY = 'editor-preview';

export const loadLevelText = async (folder: string, levelId: number): Promise<string> => {
    // Preview dal Level Editor: carica da localStorage (condiviso tra tab)
    if (folder === PREVIEW_FOLDER) {
        const saved = localStorage.getItem(PREVIEW_LOCAL_KEY);
        if (!saved) throw new Error('Nessun livello di preview in localStorage. Usa il pulsante "Testa in gioco" nel Level Editor.');
        return saved;
    }
    const url = `${AssetsRoot}assets/levels/${folder}/${levelId}.mark`;
    const response = await fetch(url);
    if (!response.ok) throw new Error(`Failed to load level ${folder}/${levelId}`);
    return response.text();
};
