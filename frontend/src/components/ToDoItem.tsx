import type { TodoItemProps } from './ToDoItemProps';

export default function TodoItem({ todo, onDelete }: TodoItemProps) {
    return (
        <li className="todo-item">
            {todo.text}
            <button className="delete-todo-btn" onClick={onDelete} title="Delete todo">
                ✕
            </button>
        </li>
    );
}
