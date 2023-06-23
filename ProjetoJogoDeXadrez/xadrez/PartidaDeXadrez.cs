using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using tabuleiro;

namespace xadrez
{
    class PartidaDeXadrez
    {
        public Tabuleiro tab {get; private set;}
        public int turno { get; private set; }
        public Cor jogadorAtual { get; private set; }
        public bool terminada { get; private set;}
        private HashSet<Peca> pecas;
        private HashSet<Peca> capturadas;
        public bool xeque { get; private set; }

        public PartidaDeXadrez()
        {
            tab = new Tabuleiro(8, 8);
                turno = 1;
            jogadorAtual = Cor.Branca;
            terminada = false;
            pecas = new HashSet<Peca>();
            capturadas= new HashSet<Peca>();
            colocarPecas();
        }

        public Peca executaMovimento(Posicao origem, Posicao destino)
        {
            Peca p = tab.retirarPeca(origem);
            p.incrementarQteMovimentos();
            Peca pecaCapturada = tab.retirarPeca(destino);
            tab.colocarPeca(p, destino);
            if ( pecaCapturada != null)
            {
                capturadas.Add(pecaCapturada);
            }

            // jogada especial roque pequeno
            if (p is Rei && destino.coluna == origem.coluna + 2)
            {
                Posicao origemT = new Posicao(origem.linha, origem.coluna + 3);
                Posicao destinoT = new Posicao(origem.linha, destino.coluna + 1);
                Peca T = tab.retirarPeca(origemT);
                T.incrementarQteMovimentos();
                tab.colocarPeca(T, destinoT);

            }

            // jogada especial roque grande
            if (p is Rei && destino.coluna == origem.coluna - 2)
            {
                Posicao origemT = new Posicao(origem.linha, origem.coluna - 4);
                Posicao destinoT = new Posicao(origem.linha, destino.coluna - 1);
                Peca T = tab.retirarPeca(origemT);
                T.incrementarQteMovimentos();
                tab.colocarPeca(T, destinoT);
            }

                return pecaCapturada;
        }


        public void desfazMovimento(Posicao origem, Posicao destino, Peca pecaCapturada)
        {
            /// <summary>
            /// desfazer movimento em xeque.
            /// </summary>
            /// <returns>
            /// 
            /// </returns>
            Peca p = tab.retirarPeca(destino);
            p.decrementarQteMovimentos();
            if (pecaCapturada != null)
            {
                tab.colocarPeca(pecaCapturada, destino);
                capturadas.Remove(pecaCapturada);
            }
            tab.colocarPeca(p, origem);

            // jogada especial roque pequeno
            if (p is Rei && destino.coluna == origem.coluna + 2)
            {
                Posicao origemT = new Posicao(origem.linha, origem.coluna + 3);
                Posicao destinoT = new Posicao(origem.linha, destino.coluna + 1);
                Peca T = tab.retirarPeca(origemT);
                T.decrementarQteMovimentos();
                tab.colocarPeca(T, origemT);
            }

            // jogada especial roque GRANDE
            if (p is Rei && destino.coluna == origem.coluna - 2)
            {
                Posicao origemT = new Posicao(origem.linha, origem.coluna - 4);
                Posicao destinoT = new Posicao(origem.linha, destino.coluna - 1);
                Peca T = tab.retirarPeca(origemT);
                T.incrementarQteMovimentos();
                tab.colocarPeca(T, destinoT);
            }
        }

        public void realizaJogada(Posicao origem, Posicao destino)
        {
           Peca pecaCapturada =  executaMovimento(origem, destino);

            if (estaEmXeque(jogadorAtual))
            {
                desfazMovimento(origem, destino, pecaCapturada);
                throw new TabuleiroException("Você não pode se colocar em xeque!");
            }

            if (estaEmXeque(adversaria(jogadorAtual)))
            {
                xeque = true;
            }
            else
            {
                xeque = false;
            }
            if (testeXequemate(adversaria(jogadorAtual)))
            {
                terminada = true;
            }
            else
            {
                turno++;
                mudaJogador();
            }

            
        }

        public void validarPosicaoDeOrigem(Posicao pos)
        {
            if (tab.peca(pos) == null)
            {
                throw new TabuleiroException("Não existe peça na posição de origem escolhida!");
            }
            if (jogadorAtual != tab.peca(pos).cor)
            {
                throw new TabuleiroException("A peça de origem escolhida não é sua!");
            }
            if (!tab.peca(pos).existeMovimentosPossiveis())
            {
                throw new TabuleiroException("Não há movimentos possíveis para a peça de origem escolhida!");
            }
        }

        public void validarPosicaoDeDestino(Posicao origem, Posicao destino)
        {
            if (!tab.peca(origem).movimentoPossivel(destino)){
                throw new TabuleiroException("Posição de destino inválida!");
            }
        }

        private void mudaJogador()
        {
            if (jogadorAtual == Cor.Branca)
            {
                jogadorAtual = Cor.Preta;
            }
            else
            {
                jogadorAtual = Cor.Branca;
            }
        }

        public HashSet<Peca> pecasCapturadas(Cor cor)
        {
            HashSet<Peca> aux = new HashSet<Peca>();
            foreach (Peca x in capturadas)
            {
                if (x.cor == cor)
                {
                    aux.Add(x);
                }
            }
            return aux;
        }

        public HashSet<Peca> pecasEmJogo(Cor cor)
        {
            HashSet<Peca> aux = new HashSet<Peca>();
            foreach (Peca x in pecas)
            {
                if (x.cor == cor)
                {
                    aux.Add(x);
                }
            }
            aux.ExceptWith(pecasCapturadas(cor));
            return aux;
        }

        // implementando cor adiversarias branca e preta.
        private Cor adversaria(Cor cor)
        {
            if (cor == Cor.Branca)
            {
                return Cor.Preta;
            }
            else
            {
                return Cor.Branca;
            }
        }

        //imdentificando a cor de uma determinada peça (Rei)
        private Peca rei(Cor cor)
        {
            foreach (Peca x in pecasEmJogo(cor))
            {
                if (x is Rei)
                {
                    return x;
                }
            }
            return null;
        }

        // para testar se o Rei esta em Xeque

        public bool estaEmXeque(Cor cor)
        {
            Peca R = rei(cor);
            if (R == null)
            {
                throw new TabuleiroException("Não tem rei da cor " + cor + " no tabuleiro!");
            }
            foreach (Peca x in pecasEmJogo(adversaria(cor)))
            {
                bool[,] mat = x.movimentosPossiveis();
                if (mat[R.posicao.linha, R.posicao.coluna])
                {
                    return true;
                }
            }
            return false;
        }

        public bool testeXequemate( Cor cor)
        {
            if (!estaEmXeque(cor))
            {
                return false;
            }
            foreach (Peca x in pecasEmJogo(cor))
            {
                bool[,] mat = x.movimentosPossiveis();
                for (int i=0; i<tab.linhas; i++)
                {
                    for (int j=0; j<tab.colunas; j++)
                    {
                        if (mat[i, j])
                        {
                            Posicao origem = x.posicao;
                            Posicao destino = new Posicao(i, j);
                            Peca pecaCapturada = executaMovimento(origem, destino);
                            bool testeXeque = estaEmXeque(cor);
                            desfazMovimento(origem, destino, pecaCapturada);
                            if (!testeXeque)
                            {
                                return false;
                            }
                        }
                    }
                }
            }
            return true;
        }

        public void colocarNovapeca(char coluna, int linha, Peca peca)
        {
            tab.colocarPeca(peca, new PosicaoXadrez(coluna, linha).toPosicao());
            pecas.Add(peca);
        }

        public void colocarPecas()
        {
            // Brancas
            colocarNovapeca('a', 1, new Torre(tab, Cor.Branca));
            colocarNovapeca('b', 1, new Cavalo(tab, Cor.Branca));
            colocarNovapeca('c', 1, new Bispo(tab, Cor.Branca));
            colocarNovapeca('d', 1, new Dama(tab, Cor.Branca));
            colocarNovapeca('e', 1, new Rei(tab, Cor.Branca, this));
            colocarNovapeca('f', 1, new Bispo(tab, Cor.Branca));
            colocarNovapeca('g', 1, new Cavalo(tab, Cor.Branca));
            colocarNovapeca('h', 1, new Torre(tab, Cor.Branca));

            colocarNovapeca('a', 2, new Peao(tab, Cor.Branca));
            colocarNovapeca('b', 2, new Peao(tab, Cor.Branca));
            colocarNovapeca('c', 2, new Peao(tab, Cor.Branca));
            colocarNovapeca('d', 2, new Peao(tab, Cor.Branca));
            colocarNovapeca('e', 2, new Peao(tab, Cor.Branca));
            colocarNovapeca('f', 2, new Peao(tab, Cor.Branca));
            colocarNovapeca('g', 2, new Peao(tab, Cor.Branca));
            colocarNovapeca('h', 2, new Peao(tab, Cor.Branca));

            //pretas
            colocarNovapeca('a', 8, new Torre(tab, Cor.Preta));
            colocarNovapeca('b', 8, new Cavalo(tab, Cor.Preta));
            colocarNovapeca('c', 8, new Bispo(tab, Cor.Preta));
            colocarNovapeca('d', 8, new Dama(tab, Cor.Preta));
            colocarNovapeca('e', 8, new Rei(tab, Cor.Preta, this));
            colocarNovapeca('f', 8, new Bispo(tab, Cor.Preta));
            colocarNovapeca('g', 8, new Cavalo(tab, Cor.Preta));
            colocarNovapeca('h', 8, new Torre(tab, Cor.Preta));

            colocarNovapeca('a', 7, new Peao(tab, Cor.Preta));
            colocarNovapeca('b', 7, new Peao(tab, Cor.Preta));
            colocarNovapeca('c', 7, new Peao(tab, Cor.Preta));
            colocarNovapeca('d', 7, new Peao(tab, Cor.Preta));
            colocarNovapeca('e', 7, new Peao(tab, Cor.Preta));
            colocarNovapeca('f', 7, new Peao(tab, Cor.Preta));
            colocarNovapeca('g', 7, new Peao(tab, Cor.Preta));
            colocarNovapeca('h', 7, new Peao(tab, Cor.Preta));



        }
    }

}
