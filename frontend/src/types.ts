// types.ts
export interface Todo {
    id: number;
    text: string;
    created: string;
}

export interface TodoList {
    id: number;
    name: string;
    created: string;
    todos: Todo[];
}
