import { useState } from 'react';
import { api } from '../api';
import TodoItem from './ToDoItem';
import type { ListCardProps } from './ListCardProps';

export default function ListCard({
    list,
    onDeleted,
    onTodoAdded,
    onTodoDeleted,
}: ListCardProps) {
    const [input, setInput] = useState('');

    const handleAddTodo = async () => {
        if (!input.trim()) return;
        const todo = await api.addTodo(list.id, input.trim());
        onTodoAdded(todo);
        setInput('');
    };

    return (
        <div className="list-card">
            <div className="list-header">
                <h2>{list.name}</h2>

                <button
                    className="delete-list-btn"
                    onClick={async () => {
                        await api.deleteList(list.id);
                        onDeleted();
                    }}
                    title="Delete list"
                >
                    ×
                </button>
            </div>

            <div className="input-group">
                <input
                    value={input}
                    onChange={(e) => setInput(e.target.value)}
                    type="text"
                    placeholder="New item…"
                    onKeyDown={(e) => e.key === 'Enter' && handleAddTodo()}
                />
                <button className="add-todo-btn" onClick={handleAddTodo}>
                    ＋
                </button>
            </div>

            <ul className="todo-list">
                {list.todos.map((t) => (
                    <TodoItem
                        key={t.id}
                        todo={t}
                        onDelete={async () => {
                            await api.deleteTodo(t.id);
                            onTodoDeleted(t.id);
                        }}
                    />
                ))}
            </ul>
        </div>
    );
}
