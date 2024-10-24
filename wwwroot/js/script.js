const menuToggle = document.getElementById('menu-toggle');
const sidebar = document.getElementById('sidebar');
const mainContent = document.getElementById('main-content'); //indexte
const avatarToggle = document.getElementById('avatar-toggle');
const avatarMenu = document.getElementById('avatar-menu');
const addMenuToggle = document.getElementById('add-menu-toggle');
const addMenu = document.getElementById('add-menu');

menuToggle.addEventListener('click', () => {
    sidebar.classList.toggle('closed');
    mainContent.classList.toggle('full-width');
});

avatarToggle.addEventListener('click', (e) => {
    e.stopPropagation();
    avatarMenu.classList.toggle('show');
});

addMenuToggle.addEventListener('click', (e) => {
    e.stopPropagation();
    addMenu.classList.toggle('show');
});

document.addEventListener('click', (e) => {
    if (!avatarMenu.contains(e.target) && !avatarToggle.contains(e.target)) {
        avatarMenu.classList.remove('show');
    }
    if (!addMenu.contains(e.target) && !addMenuToggle.contains(e.target)) {
        addMenu.classList.remove('show');
    }
});
