export interface Todo {
    id: number;
    text: string;
}

export interface TodoList {
    id: number;
    name: string;
    todos: Todo[];
}
