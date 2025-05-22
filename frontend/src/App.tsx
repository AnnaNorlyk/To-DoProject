import { useEffect, useState } from 'react';
import { api } from './api';
import ListCard from './components/ListCard';
import type { Todo, TodoList } from './types';
import './styles/app.css';

export default function App() {
    const [lists, setLists] = useState<TodoList[]>([]);
    const [loading, setLoading] = useState(true);
    const [adding, setAdding] = useState(false);

    /* fetch all lists (and their todos) on first mount */
    useEffect(() => {
        api
            .getLists()
            .then(setLists)
            .finally(() => setLoading(false));
    }, []);

    /* create a brand-new list on the server */
    const addList = async () => {
        setAdding(true);
        try {
            const newList = await api.addList(`List ${Date.now()}`);
            setLists([...lists, { ...newList, todos: [] }]);
        } finally {
            setAdding(false);
        }
    };

    if (loading) return <p className="status">Loading…</p>;

    return (
        <div className="app-container">
            <button className="add-list-btn" onClick={addList} disabled={adding}>
                + Add List
            </button>

            <div className="lists-container">
                {lists.map((l) => (
                    <ListCard
                        key={l.id}
                        list={l}
                        onDeleted={() =>
                            setLists((curr) => curr.filter((x) => x.id !== l.id))
                        }
                        onTodoAdded={(todo: Todo) =>
                            setLists((curr) =>
                                curr.map((x) =>
                                    x.id === l.id ? { ...x, todos: [...x.todos, todo] } : x
                                )
                            )
                        }
                        onTodoDeleted={(todoId: number) =>
                            setLists((curr) =>
                                curr.map((x) =>
                                    x.id === l.id
                                        ? { ...x, todos: x.todos.filter((t) => t.id !== todoId) }
                                        : x
                                )
                            )
                        }
                    />
                ))}
            </div>
        </div>
    );
}
