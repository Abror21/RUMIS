'use client'

import { Tooltip, Typography } from "antd"
import { Bar, BarChart, Legend, ResponsiveContainer, XAxis, YAxis } from "recharts"

const {Title} = Typography

const ResourceDiagram = () => {
    const data = [
        {
          name: 'Dators',
          all: 10,
          'Izsniegts': 4,
          'Lietošanā iestādē': 3,
          'Noliktavā': 1,
          'Remontā': 2,
        },
        {
          name: 'Lorem',
          all: 12,
          'Izsniegts': 3,
          'Lietošanā iestādē': 5,
          'Noliktavā': 2,
          'Remontā': 2,
        },
        {
          name: 'Ipsum',
          all: 8,
          'Izsniegts': 5,
          'Lietošanā iestādē': 1,
          'Noliktavā': 1,
          'Remontā': 1,
        },
        
    ]
    return (
        <div className="h-[500px]">
            <Title level={4}>Resursu veidu kopsavilkums</Title>
            <ResponsiveContainer width="100%" height={450}>
                <BarChart width={150} height={40} data={data}>
                    <Bar dataKey="Izsniegts" fill="#de425b" stackId="a"/>
                    <Bar dataKey="Lietošanā iestādē" fill="#f3babc" stackId="a"/>
                    <Bar dataKey="Noliktavā" fill="#bad0af" stackId="a"/>
                    <Bar dataKey="Remontā" fill="#488f31" stackId="a"/>
                    <Legend />
                    <XAxis dataKey="name" />
                    <YAxis />
                </BarChart>
            </ResponsiveContainer>
        </div>
    )
}

export default ResourceDiagram