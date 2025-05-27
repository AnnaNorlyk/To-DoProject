import type { Todo, TodoList } from './types';
import { API_BASE } from './config';

async function json<T>(response: Response): Promise<T> {
    if (!response.ok) {
        const text = await response.text();
        throw new Error(`${response.status} ${response.statusText}: ${text}`);
    }
    return response.json() as Promise<T>;
}

if (!API_BASE) {
    throw new Error("VITE_API_BASE is not defined. Please set it in your .env file.");
}

export const api = {
    getLists: (): Promise<TodoList[]> =>
        fetch(`${API_BASE}/lists`).then(json<TodoList[]>),

    addList: (name: string): Promise<TodoList> =>
        fetch(`${API_BASE}/lists`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(name),
        }).then(json<TodoList>),

    deleteList: (id: number) =>
        fetch(`${API_BASE}/lists/${id}`, { method: 'DELETE' }).then(() => { }),

    addTodo: (listId: number, text: string): Promise<Todo> =>
        fetch(`${API_BASE}/lists/${listId}/todos`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(text),
        }).then(json<Todo>),

    deleteTodo: (todoId: number) =>
        fetch(`${API_BASE}/todos/${todoId}`, { method: 'DELETE' }).then(() => { }),


};
