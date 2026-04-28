const slides = [...document.querySelectorAll('.slide')];
const counter = document.getElementById('counter');
const prevBtn = document.getElementById('prevBtn');
const nextBtn = document.getElementById('nextBtn');

let index = 0;

function render() {
  slides.forEach((slide, i) => slide.classList.toggle('active', i === index));
  counter.textContent = `Slide ${index + 1} / ${slides.length}`;
  prevBtn.disabled = index === 0;
  nextBtn.disabled = index === slides.length - 1;
}

prevBtn.addEventListener('click', () => {
  if (index > 0) {
    index -= 1;
    render();
  }
});

nextBtn.addEventListener('click', () => {
  if (index < slides.length - 1) {
    index += 1;
    render();
  }
});

document.addEventListener('keydown', (event) => {
  if (event.key === 'ArrowRight') nextBtn.click();
  if (event.key === 'ArrowLeft') prevBtn.click();
});

mermaid.initialize({ startOnLoad: true, theme: 'dark' });
render();
