export function highlightRoom(roomId) {
    document.querySelectorAll('.highlighted').forEach(el => {
        el.classList.remove('highlighted');
    });

    const room = document.querySelector(`[data-room="${roomId}"]`);
    if (room) {
        room.classList.add('highlighted');
        room.scrollIntoView({ behavior: 'smooth', block: 'center' });
    }
}