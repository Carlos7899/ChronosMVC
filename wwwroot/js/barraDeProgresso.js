// Função para animar a barra de progresso
function animarBarraProgresso(progresso) {
    const progressBar = document.querySelector('.progress-bar');
    progressBar.style.transition = "width 1s ease-out";  // Definir o tempo de transição
    progressBar.style.width = progresso + '%';
}

// Função para mudar as classes dos botões de etapas
function atualizarEtapas(etapa) {
    const botoes = document.querySelectorAll('.step-btn');

    botoes.forEach((botao, index) => {
        if (index < etapa) {
            botao.classList.remove('btn-secondary');
            botao.classList.add('btn-primary');
        } else if (index === etapa) {
            botao.classList.remove('btn-primary');
            botao.classList.add('btn-warning');  // Status de "em progresso"
        } else {
            botao.classList.remove('btn-primary', 'btn-warning');
            botao.classList.add('btn-secondary');
        }
    });
}

// Função para avançar nas etapas
function avancarEtapa() {
    let etapaAtual = parseInt(document.querySelector('.step-btn.btn-warning').textContent);
    if (etapaAtual < 3) {
        etapaAtual++;
        animarBarraProgresso(etapaAtual * 33); // Atualiza a barra com a porcentagem baseada na etapa
        atualizarEtapas(etapaAtual);
    }
}

// Função para retroceder na etapa
function retrocederEtapa() {
    let etapaAtual = parseInt(document.querySelector('.step-btn.btn-warning').textContent);
    if (etapaAtual > 1) {
        etapaAtual--;
        animarBarraProgresso(etapaAtual * 33);
        atualizarEtapas(etapaAtual);
    }
}

// Configuração de eventos para avançar e retroceder
document.querySelector('#avancar').addEventListener('click', avancarEtapa);
document.querySelector('#retroceder').addEventListener('click', retrocederEtapa);
