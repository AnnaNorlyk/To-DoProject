import type { Todo, TodoList } from '../types';

export interface ListCardProps {
    list: TodoList;
    onDeleted: () => void;
    onTodoAdded: (todo: Todo) => void;
    onTodoDeleted: (todoId: number) => void;
}
