'use client'

import { Tooltip, Typography } from "antd"
import { Bar, BarChart, Legend, ResponsiveContainer, XAxis, YAxis } from "recharts"

const {Title} = Typography

const ApplicationDynamicDiagram = () => {
    const data = [
        {
          name: '25.11',
          all: 5,
          'Iesniegti': 1,
          'Atlikta izskatīšana': 1,
          'Piešķirti resursi': 2,
          'Noraidīti': 1,
        },
        {
          name: '26.11',
          all: 7,
          'Iesniegti': 3,
          'Atlikta izskatīšana': 0,
          'Piešķirti resursi': 2,
          'Noraidīti': 2,
        },
        {
          name: '27.11',
          all: 2,
          'Iesniegti': 0,
          'Atlikta izskatīšana': 0,
          'Piešķirti resursi': 1,
          'Noraidīti': 1,
        },
        {
          name: '28.11',
          all: 3,
          'Iesniegti': 1,
          'Atlikta izskatīšana': 0,
          'Piešķirti resursi': 2,
          'Noraidīti': 0,
        },
    ]
    return (
        <div className="h-[500px]">
            <Title level={4}>Pieteikumu dinamika</Title>
            <ResponsiveContainer width="100%" height={450}>
                <BarChart width={150} height={40} data={data}>
                    <Bar dataKey="Iesniegti" fill="#de425b" stackId="a"/>
                    <Bar dataKey="Atlikta izskatīšana" fill="#f3babc" stackId="a"/>
                    <Bar dataKey="Piešķirti resursi" fill="#bad0af" stackId="a"/>
                    <Bar dataKey="Noraidīti" fill="#488f31" stackId="a"/>
                    <Legend />
                    <XAxis dataKey="name" />
                    <YAxis />
                </BarChart>
            </ResponsiveContainer>
        </div>
    )
}

export default ApplicationDynamicDiagram