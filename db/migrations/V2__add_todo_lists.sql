CREATE TABLE todolists (
    id          INT AUTO_INCREMENT PRIMARY KEY,
    name        VARCHAR(255) NOT NULL,
    created_at  TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);
e
ALTER TABLE todos
    ADD COLUMN todo_list_id INT NULL;


ALTER TABLE todos
    MODIFY todo_list_id INT NOT NULL,
    ADD CONSTRAINT fk_todos_todolists
        FOREIGN KEY (todo_list_id)
        REFERENCES todolists(id)
        ON DELETE CASCADE;
