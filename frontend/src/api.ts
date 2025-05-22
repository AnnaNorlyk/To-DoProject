import type { Todo, TodoList } from './types';

async function json<T>(response: Response): Promise<T> {
    if (!response.ok) {
        const text = await response.text();
        throw new Error(`${response.status} ${response.statusText}: ${text}`);
    }
    return response.json() as Promise<T>;
}

export const api = {
    getLists: (): Promise<TodoList[]> =>
        fetch('/api/lists').then(json<TodoList[]>),

    addList: (name: string): Promise<TodoList> =>
        fetch('/api/lists', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(name),
        }).then(json<TodoList>),

    deleteList: (id: number) =>
        fetch(`/api/lists/${id}`, { method: 'DELETE' }).then(() => { }),

    addTodo: (listId: number, text: string): Promise<Todo> =>
        fetch(`/api/lists/${listId}/todos`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(text),
        }).then(json<Todo>),

    deleteTodo: (todoId: number) =>
        fetch(`/api/lists/todos/${todoId}`, { method: 'DELETE' }).then(() => { }),
};
