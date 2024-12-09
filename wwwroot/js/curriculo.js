document.addEventListener("DOMContentLoaded", function () {

    // Busca o nome do egresso a partir do atributo data do div
    var nomeEgresso = document.getElementById("nomeEgresso").getAttribute("data-nome");

    // Verificação das bibliotecas
    if (typeof window.jspdf === 'undefined') {
        console.error("jsPDF não carregado corretamente!");
    } else {
        console.log("jsPDF carregado!");
    }

    if (typeof window.docx === 'undefined') {
        console.error("docx não carregado corretamente!");
    } else {
        console.log("docx carregado!");
    }

    if (typeof window.saveAs === 'undefined') {
        console.error("FileSaver.js não carregado corretamente!");
    } else {
        console.log("FileSaver.js carregado!");
    }

    // Botão de download PDF
    const downloadPdfButton = document.getElementById("download-pdf");
    if (downloadPdfButton) {
        downloadPdfButton.addEventListener("click", function () {
            const { jsPDF } = window.jspdf;
            const doc = new jsPDF();

            // Cabeçalho do currículo
            doc.setFontSize(18);
            doc.text( nomeEgresso, 20, 20);  // Agora o nome do egresso aparecerá

            // Informações Pessoais
            doc.setFontSize(12);
            doc.text("Email: " + document.getElementById("email").innerText, 20, 40);
            doc.text("Telefone: " + document.getElementById("telefone").innerText, 20, 50);
            doc.text("Habilidades: " + document.getElementById("habilidades").innerText, 20, 60);
            doc.text("Objetivo: " + document.getElementById("descricao").innerText, 20, 70);

            // Adicionar uma linha separadora
            doc.line(20, 75, 190, 75); // Linha horizontal

            // Experiências Profissionais
            doc.setFontSize(14);
            doc.text("Experiências Profissionais", 20, 85);
            let y = 95;
            const experiencias = document.getElementsByClassName("experiencia-item");
            Array.from(experiencias).forEach(function (experiencia) {
                doc.setFontSize(12);
                doc.text("Cargo: " + experiencia.querySelector(".cargo").innerText, 20, y);
                y += 10;
                doc.text("Empresa: " + experiencia.querySelector(".empresa").innerText, 20, y);
                y += 10;
                doc.text("Período: " + experiencia.querySelector(".periodo").innerText, 20, y);
                y += 10;
                doc.text("Descrição: " + experiencia.querySelector(".descricao").innerText, 20, y);
                y += 20; // Espaço entre experiências
            });

            // Adicionar uma linha separadora
            doc.line(20, y, 190, y); // Linha horizontal

            // Formações Acadêmicas
            y += 20;
            doc.setFontSize(14);
            doc.text("Formações Acadêmicas", 20, y);
            y += 10;
            const formacoes = document.getElementsByClassName("formacao-item");
            Array.from(formacoes).forEach(function (formacao) {
                doc.setFontSize(12);
                doc.text("Curso: " + formacao.querySelector(".curso").innerText, 20, y);
                y += 10;
                doc.text("Instituição: " + formacao.querySelector(".instituicao").innerText, 20, y);
                y += 10;
                doc.text("Data de Conclusão: " + formacao.querySelector(".dataConclusao").innerText, 20, y);
                y += 15; // Espaço entre formações
            });

            // Gerar o PDF e baixar com o nome correto
            doc.save("curriculo_" + nomeEgresso + ".pdf");  // Usando o nome do egresso no nome do arquivo
        });
    }

    // Botão de download DOCX
    const downloadDocxButton = document.getElementById("download-docx");
    if (downloadDocxButton) {
        downloadDocxButton.addEventListener("click", function () {
            // Criar o documento DOCX
            const doc = new docx.Document({
                sections: [
                    {
                        properties: {},
                        children: [
                            // Cabeçalho
                            new docx.Paragraph({
                                text: nomeEgresso,  // Nome do egresso aqui também
                                heading: docx.HeadingLevel.HEADING_1,
                                alignment: docx.AlignmentType.CENTER,
                            }),
                            new docx.Paragraph({
                                text: "Informações Pessoais:",
                                heading: docx.HeadingLevel.HEADING_2,
                            }),

                            // Informações Pessoais
                            new docx.Paragraph("Email: " + document.getElementById("email").innerText),
                            new docx.Paragraph("Telefone: " + document.getElementById("telefone").innerText),
                            new docx.Paragraph("Habilidades: " + document.getElementById("habilidades").innerText),
                            new docx.Paragraph("Descrição: " + document.getElementById("descricao").innerText),

                            new docx.Paragraph({
                                text: "Experiências Profissionais:",
                                heading: docx.HeadingLevel.HEADING_2,
                            }),
                            ...getExperiencias(), // Adiciona as experiências

                            new docx.Paragraph({
                                text: "Formações Acadêmicas:",
                                heading: docx.HeadingLevel.HEADING_2,
                            }),
                            ...getFormacoes(), // Adiciona as formações
                        ]
                    }
                ]
            });

            // Gerar o arquivo DOCX e forçar o download com o nome correto
            docx.Packer.toBlob(doc).then((blob) => {
                saveAs(blob, "curriculo_" + nomeEgresso + ".docx");  // Usando o nome do egresso no nome do arquivo
            }).catch(error => {
                console.error("Erro ao gerar o arquivo DOCX", error);
                alert("Houve um erro ao tentar gerar o DOCX. Tente novamente mais tarde.");
            });
        });
    }

    // Função para pegar as experiências profissionais
    function getExperiencias() {
        const experiencias = document.getElementsByClassName("experiencia-item");
        const experienciasArray = Array.from(experiencias);
        return experienciasArray.map((experiencia) => {
            return [
                new docx.Paragraph("Cargo: " + experiencia.querySelector(".cargo").innerText),
                new docx.Paragraph("Empresa: " + experiencia.querySelector(".empresa").innerText),
                new docx.Paragraph("Período: " + experiencia.querySelector(".periodo").innerText),
                new docx.Paragraph("Descrição: " + experiencia.querySelector(".descricao").innerText),
                new docx.Paragraph(" "), // Espaço entre experiências
            ];
        }).flat(); // Usamos flat() para achatar o array de parágrafos
    }

    // Função para pegar as formações acadêmicas
    function getFormacoes() {
        const formacoes = document.getElementsByClassName("formacao-item");
        const formacoesArray = Array.from(formacoes);
        return formacoesArray.map((formacao) => {
            return [
                new docx.Paragraph("Curso: " + formacao.querySelector(".curso").innerText),
                new docx.Paragraph("Instituição: " + formacao.querySelector(".instituicao").innerText),
                new docx.Paragraph("Data de Conclusão: " + formacao.querySelector(".dataConclusao").innerText),
                new docx.Paragraph(" "), // Espaço entre formações
            ];
        }).flat(); // Usamos flat() para achatar o array de parágrafos
    }
});
