import React, { useState } from 'react';
import './App.css';

const App: React.FC = () => {
    const [todos, setTodos] = useState<string[]>([]);
    const [input, setInput] = useState<string>('');

    const addTodo = () => {
        const text = input.trim();
        if (!text) return;
        setTodos([...todos, text]);
        setInput('');
    };

    return (
        <div className="app-container">
            <h1>To-Do List</h1>

            <div className="input-group">
                <input
                    type="text"
                    value={input}
                    onChange={(e) => setInput(e.target.value)}
                    placeholder="What needs to be done?"
                />
                <button onClick={addTodo}>Add</button>
            </div>

            <ul className="todo-list">
                {todos.map((todo, idx) => (
                    <li key={idx} className="todo-item">
                        {todo}
                    </li>
                ))}
            </ul>
        </div>
    );
};

export default App;
