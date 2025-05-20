import { useState } from 'react';
import './App.css';

type Todo = { id: number; text: string };
type TodoList = { id: number; name: string; todos: Todo[] };

let nextListId = 1;
let nextTodoId = 1;

export default function App() {
    const [lists, setLists] = useState<TodoList[]>([
        { id: nextListId++, name: 'List 1', todos: [] }
    ]);

    // Add a new empty list
    const addList = () => {
        setLists([
            ...lists,
            { id: nextListId++, name: `List ${nextListId - 1}`, todos: [] }
        ]);
    };

    // Delete an entire list by id
    const deleteList = (listId: number) => {
        setLists(lists.filter(l => l.id !== listId));
    };

    // Add a todo to a specific list
    const addTodo = (listId: number, text: string) => {
        setLists(lists.map(l => {
            if (l.id !== listId) return l;
            const todo: Todo = { id: nextTodoId++, text };
            return { ...l, todos: [...l.todos, todo] };
        }));
    };

    // Delete a todo from a specific list
    const deleteTodo = (listId: number, todoId: number) => {
        setLists(lists.map(l => {
            if (l.id !== listId) return l;
            return { ...l, todos: l.todos.filter(t => t.id !== todoId) };
        }));
    };

    return (
        <div className="app-container">
            <button className="add-list-btn" onClick={addList}>
                + Add List
            </button>

            <div className="lists-container">
                {lists.map(list => (
                    <div key={list.id} className="list-card">
                        <div className="list-header">
                            <h2>{list.name}</h2>
                            <button
                                className="delete-list-btn"
                                onClick={() => deleteList(list.id)}
                            >
                                ×
                            </button>
                        </div>

                        <div className="input-group">
                            <input
                                type="text"
                                placeholder="New item…"
                                onKeyDown={e => {
                                    if (e.key === 'Enter' && e.currentTarget.value.trim()) {
                                        addTodo(list.id, e.currentTarget.value.trim());
                                        e.currentTarget.value = '';
                                    }
                                }}
                            />
                        </div>

                        <ul className="todo-list">
                            {list.todos.map(todo => (
                                <li key={todo.id} className="todo-item">
                                    {todo.text}
                                    <button
                                        className="delete-todo-btn"
                                        onClick={() => deleteTodo(list.id, todo.id)}
                                    >
                                        ✕
                                    </button>
                                </li>
                            ))}
                        </ul>
                    </div>
                ))}
            </div>
        </div>
    );
}
