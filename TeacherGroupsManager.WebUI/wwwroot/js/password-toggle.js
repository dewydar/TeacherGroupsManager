document.addEventListener('click', event => {
    const button = event.target.closest('[data-password-toggle]');
    if (!button) return;

    const group = button.closest('.password-input-group');
    const input = group?.querySelector('input');
    if (!input) return;

    const isVisible = input.type === 'text';
    input.type = isVisible ? 'password' : 'text';
    button.setAttribute('aria-pressed', String(!isVisible));
    button.querySelector('[data-password-icon-show]')?.classList.toggle('d-none', !isVisible);
    button.querySelector('[data-password-icon-hide]')?.classList.toggle('d-none', isVisible);
});
