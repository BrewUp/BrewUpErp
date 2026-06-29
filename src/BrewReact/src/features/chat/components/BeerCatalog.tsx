import { useEffect, useState } from 'react'
import { getBeerCatalog } from '../services/chatApiService'
import type { BeerCatalogItem } from '../types/chat.types'

function BeerCatalog() {
  const [beers, setBeers] = useState<BeerCatalogItem[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    getBeerCatalog()
      .then(setBeers)
      .catch((err: Error) => setError(err.message))
      .finally(() => setLoading(false))
  }, [])

  if (loading) {
    return <div aria-label="Caricamento catalogo" role="status" />
  }

  if (error) {
    return <p className="beer-catalog__error">{error}</p>
  }

  if (beers.length === 0) {
    return <p className="beer-catalog__empty">Nessun elemento trovato</p>
  }

  return (
    <table className="beer-catalog">
      <thead>
        <tr>
          <th>Birra</th>
          <th>Stile</th>
          <th>ABV</th>
          <th>Attivo</th>
        </tr>
      </thead>
      <tbody>
        {beers.map((beer) => (
          <tr key={beer.beerId ?? beer.name}>
            <td>{beer.name ?? '—'}</td>
            <td>{beer.style ?? '—'}</td>
            <td>{beer.alcoholByVolume != null ? beer.alcoholByVolume : '—'}</td>
            <td>{beer.isActive ? 'Sì' : 'No'}</td>
          </tr>
        ))}
      </tbody>
    </table>
  )
}

export default BeerCatalog
