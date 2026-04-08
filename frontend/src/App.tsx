import { Routes, Route, Navigate } from 'react-router-dom'

function App() {
  return (
    <Routes>
      {/* Rutas se agregan después */}
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  )
}

export default App