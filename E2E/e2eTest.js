import { Selector, ClientFunction } from 'testcafe';

const BASE_URL = process.env.STAGING_REACT_URL;
if (!BASE_URL) {
    throw new Error('Environment variable STAGING_REACT_URL has to be set');
}

const getLocation = ClientFunction(() => window.location.href);

fixture`Todo App E2E Tests`
    .page(BASE_URL);

test('App loads successfully', async t => {
    await t.expect(getLocation()).contains(BASE_URL, 'Navigated to the correct URL');
    const todoInput = Selector('input.new-todo');
    await t.expect(todoInput.exists).ok('New-todo input is present');
});

test('Can add and remove a todo', async t => {
    const todoInput = Selector('input.new-todo');
    const todoList  = Selector('ul.todo-list');
    const firstItem = todoList.find('li').nth(0);
    const deleteBtn = firstItem.find('button.destroy');

    // Add a new todo
    const text = 'TestCafe E2E ' + new Date().toISOString();
    await t
        .typeText(todoInput, text, { replace: true })
        .pressKey('enter');

    // Verify it appears in the list
    await t.expect(firstItem.innerText).contains(text, 'New todo is added to the list');

    // Delete it
    await t.hover(firstItem);
    await t.click(deleteBtn);

    // Verify it's gone
    await t.expect(todoList.find('li').withText(text).exists).notOk('Todo was removed');
});
