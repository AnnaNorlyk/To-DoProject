import type { Todo } from '../types';

export interface TodoItemProps {
    todo: Todo;
    onDelete: () => void;
}
