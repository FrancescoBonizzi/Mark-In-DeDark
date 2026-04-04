import { parseLevel } from '../../grid/GridLoader';
import { serializeMark } from '../../grid/GridSerializer';
import type { EditorState } from './editorState';
import { createStateFromBundle, isDirty, markSaved } from './editorState';

/**
 * Apre un file .mark dal filesystem locale (File System Access API se
 * disponibile, altrimenti <input type="file"> classico).
 * Ritorna il nuovo EditorState o null se l'utente ha annullato.
 */
export async function openFile(): Promise<{ state: EditorState; filename: string } | null> {
    return new Promise(resolve => {
        const input = document.createElement('input');
        input.type = 'file';
        input.accept = '.mark';

        input.onchange = () => {
            const file = input.files?.[0];
            if (!file) {
                resolve(null);
                return;
            }
            const reader = new FileReader();
            reader.onload = () => {
                try {
                    const text = reader.result as string;
                    const bundle = parseLevel(text);
                    const state = createStateFromBundle(bundle, file.name);
                    resolve({ state, filename: file.name });
                } catch (err) {
                    console.error('[LevelEditor] Errore parsing .mark:', err);
                    window.alert(`Errore nel leggere il file:\n${String(err)}`);
                    resolve(null);
                }
            };
            reader.onerror = () => {
                window.alert('Errore nel leggere il file.');
                resolve(null);
            };
            reader.readAsText(file);
        };

        // onchange non scatta se l'utente annulla; usiamo un timeout o focus
        input.oncancel = () => resolve(null);

        input.click();
    });
}

/**
 * Salva il livello corrente come file .mark tramite download.
 */
export function saveFile(state: EditorState): void {
    const content = serializeMark(state.grid);
    const blob = new Blob([content], { type: 'text/plain' });
    const url = URL.createObjectURL(blob);

    const a = document.createElement('a');
    a.href = url;
    a.download = state.currentFilename;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);

    markSaved(state);
}

/**
 * Mostra confirm se ci sono modifiche non salvate.
 * Ritorna true se si può procedere (nessuna modifica o utente ha confermato).
 */
export function confirmIfDirty(state: EditorState): boolean {
    if (!isDirty(state)) return true;
    return window.confirm('Ci sono modifiche non salvate. Procedere senza salvare?');
}
