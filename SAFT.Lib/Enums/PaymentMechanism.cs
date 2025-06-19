namespace SAFT.Lib
{
    /// <summary>
    /// Restricao: CC para Cartao credito, CD para Cartao debito, CH para Cheque bancario, CI para credito documentario internacional, CO para Cheque ou cartao oferta, CS para Compensacao de saldos em conta corrente, DE para Dinheiro eletronico, por exemplo em cartoes de fidelidade ou de pontos, LC para Letra comercial, MB para Referencias de pagamento para Multibanco, NU para Numerario, OU para Outros meios aqui nao assinalados, PR para Permuta de bens, TB para Transferencia bancaria ou debito direto autorizado, TR para titulos de compensacao extrassalarial independentemente do seu suporte, por exemplo, titulos de refeicao, educacao, etc.
    /// </summary>
    public enum PaymentMechanism
    {
        CC, // Cartao credito
        CD, // Cartao debito
        CH, // Cheque bancario
        CI, // Credito documentario internacional
        CO, // Cheque ou cartao oferta
        CS, // Compensacao de saldos em conta corrente
        DE, // Dinheiro eletronico
        LC, // Letra comercial
        MB, // Referencias de pagamento para Multibanco
        NU, // Numerario
        OU, // Outros meios aqui nao assinalados
        PR, // Permuta de bens
        TB, // Transferencia bancaria ou debito direto autorizado
        TR  // Titulos de compensacao extrassalarial
    }
} 